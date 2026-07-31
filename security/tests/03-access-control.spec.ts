import { test, expect } from './helpers/fixtures';
import { API_BASE } from './helpers/env';

function auth(token: string) {
  return { Authorization: `Bearer ${token}` };
}

test.describe('Sécurité — contrôle d’accès & IDOR', () => {
  test('un utilisateur ne peut pas lire la candidature d’un autre (IDOR read)', async ({ users }) => {
    // A crée une candidature
    const create = await users.apiA.post('/api/candidatures', {
      headers: auth(users.a.accessToken),
      data: { title: 'Poste secret A', contractType: 0, entrepriseName: 'Acme A' },
    });
    expect(create.status(), 'création par A → 201').toBe(201);
    const candidature = await create.json();

    // B tente de la lire
    const asB = await users.apiB.get(`/api/candidatures/${candidature.id}`, {
      headers: auth(users.b.accessToken),
    });
    expect(asB.status(), 'B ne doit PAS accéder à la ressource de A (404 attendu)').toBe(404);
    expect(asB.status(), 'surtout pas 200').not.toBe(200);
  });

  test('un utilisateur ne peut pas modifier la candidature d’un autre (IDOR update)', async ({
    users,
  }) => {
    const create = await users.apiA.post('/api/candidatures', {
      headers: auth(users.a.accessToken),
      data: { title: 'À modifier', contractType: 0, entrepriseName: 'Acme A2' },
    });
    const candidature = await create.json();

    const asB = await users.apiB.put(`/api/candidatures/${candidature.id}`, {
      headers: auth(users.b.accessToken),
      data: {
        title: 'Piraté par B',
        contractType: 0,
        status: 0,
        entrepriseId: candidature.entrepriseId,
      },
    });
    expect(asB.status(), 'update croisé refusé (404)').toBe(404);
  });

  test('un utilisateur ne peut pas supprimer la candidature d’un autre (IDOR delete)', async ({
    users,
  }) => {
    const create = await users.apiA.post('/api/candidatures', {
      headers: auth(users.a.accessToken),
      data: { title: 'À supprimer', contractType: 0, entrepriseName: 'Acme A3' },
    });
    const candidature = await create.json();

    const asB = await users.apiB.delete(`/api/candidatures/${candidature.id}`, {
      headers: auth(users.b.accessToken),
    });
    expect(asB.status(), 'delete croisé refusé (404)').toBe(404);

    // Sanity : A la voit toujours
    const stillThere = await users.apiA.get(`/api/candidatures/${candidature.id}`, {
      headers: auth(users.a.accessToken),
    });
    expect(stillThere.ok(), 'la candidature de A existe toujours').toBeTruthy();
  });

  test('un utilisateur non-admin ne peut pas accéder à l’admin (403)', async ({ users }) => {
    const res = await users.apiB.get('/api/admin/users', { headers: auth(users.b.accessToken) });
    expect(res.status(), 'non-admin → 403').toBe(403);
  });

  test('l’admin est inaccessible sans authentification (401)', async ({ playwright }) => {
    const anon = await playwright.request.newContext({ baseURL: API_BASE, ignoreHTTPSErrors: true });
    const res = await anon.get('/api/admin/users');
    expect(res.status(), 'anonyme → 401').toBe(401);
    await anon.dispose();
  });
});
