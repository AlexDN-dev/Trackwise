#!/usr/bin/env bash
set -euo pipefail

# =========================================================================
# Trackwise — Provisioning Azure OpenAI (resource group + service + model)
# =========================================================================
# Usage : ./setup-azure-openai.sh
# Prérequis : az CLI logged in (az login), subscription active
# =========================================================================

# --- À personnaliser -----------------------------------------------------
RESOURCE_GROUP="trackwise-rg"
LOCATION="germanywestcentral"
ACCOUNT_NAME="trackwise-openai"    # nom unique dans ta souscription
MODEL="gpt-5-nano".                # cheap + capable, parfait pour extraction JSON
DEPLOYMENT_NAME="gpt-5-nano"       # nom du déploiement (utilisé dans le client)
CAPACITY=10                        # k tokens/min. 10 = ~10k TPM, largement assez pour tester
# ------------------------------------------------------------------------

# Attend que le RG disparaisse si une suppression est déjà en cours
TIMEOUT=300  # 5 min max
ELAPSED=0
if az group show --name "$RESOURCE_GROUP" &>/dev/null; then
  echo "Le Resource Group existe déjà, vérification de son statut..."
fi

until ! az group show --name "$RESOURCE_GROUP" &>/dev/null; do
  if [ $ELAPSED -ge $TIMEOUT ]; then
    echo "❌ Timeout : RG toujours présent après ${TIMEOUT}s"
    exit 1
  fi
  echo "…toujours en suppression (${ELAPSED}s)"
  sleep 10
  ELAPSED=$((ELAPSED + 10))
done

echo "RG absent ou supprimé, on lance la création."
echo "==> Resource group : $RESOURCE_GROUP ($LOCATION)"
az group create \
  --name "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --output none

echo "==> Compte Azure OpenAI : $ACCOUNT_NAME"
az cognitiveservices account create \
  --name "$ACCOUNT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --kind OpenAI \
  --sku S0 \
  --yes \
  --output none

echo "==> Sélection de la dernière version GA de $MODEL disponible..."
MODEL_VERSION=$(az cognitiveservices account list-models \
  --name "$ACCOUNT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query "reverse(sort_by([?name=='$MODEL' && lifecycleStatus=='GenerallyAvailable'], &version))[0].version" \
  -o tsv)

if [ -z "$MODEL_VERSION" ] || [ "$MODEL_VERSION" == "None" ]; then
  echo "❌ Aucune version GA de '$MODEL' trouvée dans $LOCATION."
  echo "   Versions dispos :"
  az cognitiveservices account list-models \
    --name "$ACCOUNT_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query "[?name=='$MODEL'].{version:version, status:lifecycleStatus}" \
    -o table
  exit 1
fi

echo "==> Déploiement du modèle : $MODEL ($MODEL_VERSION) → $DEPLOYMENT_NAME"
az cognitiveservices account deployment create \
  --name "$ACCOUNT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --deployment-name "$DEPLOYMENT_NAME" \
  --model-name "$MODEL" \
  --model-version "$MODEL_VERSION" \
  --model-format OpenAI \
  --sku-capacity "$CAPACITY" \
  --sku-name "GlobalStandard" \
  --output none

echo ""
echo "=============================================================="
echo " ✅ Ressource prête. Récupération des credentials..."
echo "=============================================================="

ENDPOINT=$(az cognitiveservices account show \
  --name "$ACCOUNT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query "properties.endpoint" -o tsv)

API_KEY=$(az cognitiveservices account keys list \
  --name "$ACCOUNT_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query "key1" -o tsv)

echo ""
echo "  Endpoint       : $ENDPOINT"
echo "  Deployment     : $DEPLOYMENT_NAME"
echo "  API Key        : $API_KEY"
echo ""
echo "=============================================================="
echo " Enregistre la clé en user-secrets :"
echo "=============================================================="
echo ""
echo "  cd backend/src/Trackwise.API"
echo "  dotnet user-secrets set \"AzureOpenAi:Endpoint\" \"$ENDPOINT\""
echo "  dotnet user-secrets set \"AzureOpenAi:Deployment\" \"$DEPLOYMENT_NAME\""
echo "  dotnet user-secrets set \"AzureOpenAi:ApiKey\" \"$API_KEY\""
echo ""
