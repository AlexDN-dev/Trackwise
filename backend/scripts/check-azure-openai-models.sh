#!/usr/bin/env bash
set -euo pipefail

# =========================================================================
# Trackwise — Scan des modèles OpenAI disponibles par région
# =========================================================================
# Usage : ./check-azure-openai-models.sh
# Prérequis : az CLI logged in (az login)
# =========================================================================

# --- Régions à scanner (tes régions autorisées) ---
REGIONS=(
  "spaincentral"
  "uaenorth"
  "polandcentral"
  "italynorth"
  "germanywestcentral"
)

# --- Modèles pertinents pour de l'extraction JSON ---
# Tous ceux de la gamme "cheap" chez OpenAI/Azure. Le script montre chaque
# version dispo avec son statut (GA, Preview, Deprecating).
MODELS=(
  "gpt-4o-mini"
  "gpt-4.1-mini"
  "gpt-4.1-nano"
  "gpt-5-mini"
  "gpt-5-nano"
  "gpt-5-chat"
  "o4-mini"
  "o3-mini"
  "o1-mini"
  "gpt-4o"
  "gpt-4.1"
  "gpt-35-turbo"
  "gpt-35-turbo-16k"
)

echo ""
echo "======================================================================"
echo " Scan Azure OpenAI — versions GA par région"
echo "======================================================================"
echo ""

# JMESPath : filtre sur nos modèles cibles, TOUS statuts (GA + Preview + Deprecating)
# On exclut juste les modèles complètement retirés (status vide)
MODELS_FILTER=$(printf "'%s'," "${MODELS[@]}" | sed 's/,$//')
JMES="[?contains([$MODELS_FILTER], model.name)].{name:model.name, version:model.version, status:model.lifecycleStatus}"

FOUND_ANY=0

for region in "${REGIONS[@]}"; do
  echo "─────────────────────────────────────────"
  echo " Région : $region"
  echo "─────────────────────────────────────────"

  RESULT=$(az cognitiveservices model list \
    --location "$region" \
    --query "$JMES" \
    -o table 2>/dev/null || true)

  if [ -z "$RESULT" ] || [ "$RESULT" == "[]" ]; then
    echo " (aucun modèle GA parmi ceux ciblés)"
  else
    echo "$RESULT"
    FOUND_ANY=1
  fi
  echo ""
done

echo "======================================================================"
if [ $FOUND_ANY -eq 0 ]; then
  echo " ❌ Aucun modèle de la liste trouvé dans aucune région autorisée."
  echo "    Pistes :"
  echo "    1) Élargir MODELS ci-dessus si tu vises des modèles plus gros"
  echo "    2) Demander à Azure Support de débloquer une région populaire"
  echo "       (swedencentral, eastus, westeurope) pour plus de choix"
else
  echo " Légende statuts :"
  echo "   - GenerallyAvailable : recommandé, stable, pas de deprecation prévue"
  echo "   - Preview            : utilisable mais peut changer, moins garanti"
  echo "   - Deprecating        : à éviter, ne peut plus être déployé"
  echo ""
  echo " Priorise GA > Preview > Deprecating."
  echo " Puis dans setup-azure-openai.sh :"
  echo "   - LOCATION = région choisie"
  echo "   - MODEL    = nom du modèle"
  echo "   (la version GA la plus récente est auto-détectée)"
fi
echo "======================================================================"
