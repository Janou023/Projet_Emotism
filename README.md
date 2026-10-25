# EMOTISM

> Jeu vidéo  pour aider les personnes avec un Trouble du Spectre de l'Autisme (TSA) à reconnaître, comprendre et réguler leurs émotions, ainsi que celles des autres.

Projet transpromotionnel (1A et 2A) de l'**École Nationale Supérieure de Cognitique**, réalisé au premier semestre 2026-2027.


---

## Sommaire

- [Contexte](#contexte)
- [Objectifs](#objectifs)
- [Le jeu](#le-jeu)
- [Public visé et accessibilité](#public-visé-et-accessibilité)
- [Installation](#installation)
- [Organisation du dépôt](#organisation-du-dépôt)
- [Calendrier](#calendrier)
- [Équipe](#équipe)
- [Contribuer](#contribuer)

---

## Contexte

En France, les troubles du spectre de l'autisme touchent environ 1 personne sur 100. Beaucoup de personnes TSA rencontrent des difficultés pour reconnaître, exprimer et réguler leurs propres émotions (alexithymie) et pour décoder celles des autres. Ces difficultés peuvent provoquer du stress, des surcharges sensorielles ou émotionnelles, et freiner la socialisation.

EMOTISM explore comment le jeu vidéo, un outil ludique et familier, peut accompagner ces personnes. Il s'inscrit dans la lignée des jeux à visée thérapeutique ou éducative (comme EndeavorRx pour le TDAH).

## Objectifs

- Concevoir un **jeu fonctionnel** destiné aux personnes TSA.
- Aider le joueur à **comprendre ses émotions internes** et à **détecter celles des personnes avec qui il interagit**.
- Être conçu avec l'avis de **professionnels** (psychologues, éducateurs spécialisés) et **testé par des personnes concernées**.

## Le jeu

**Genre** : visual novel avec déplacement libre et collection de cartes émotions.

### Le monde

Le joueur se déplace sur une **carte du monde en vue 2D de dessus**. Elle indique l'emplacement des différents lieux :

- les **pôles émotions**, pour récupérer des cartes ;
- les **défis**, des scènes où il faut mettre en pratique ce qu'il a appris.

### Les pôles émotions

Le joueur assiste à un **dialogue entre deux personnages ou plus** (il est uniquement spectateur). La discussion met en scène **une émotion précise**. À la fin, il récupère la **carte émotion** correspondante.

### Les défis

Les défis se déroulent dans des niveaux en **vue 2D de côté**. En parlant à un personnage, le joueur lance une **séquence de visual novel** : un dialogue entre lui et le ou les personnages. Pendant ce dialogue, il doit **utiliser les cartes obtenues** pour réagir de façon adaptée et mener la séquence à bien.

### La collection de cartes

Le joueur consulte ses cartes dans un **classeur** (accessible depuis la pause ou lorsqu'un défi lui demande d'utiliser ses cartes).

> 🚧 Certains éléments sont encore à valider pendant la phase d'idéation (accès au classeur, possibilité de retour en arrière pendant un défi, nombre de cartes émotions, nombre de cartes du monde et de niveaux, etc.). Les détails sont suivis dans les [Issues](../../issues).

### Exigences principales

- Jeu **2D** : le joueur contrôle son personnage et interagit avec des **PNJ qui ont leurs propres émotions**.
- Émotions du joueur et des PNJ **représentées de manière explicite**.
- Commandes **simples** et difficulté **progressive**.
- **Ressources d'aide** (contacts de professionnels de santé, services d'aide) accessibles et mises en évidence dans le jeu.
- Fonctionnement fluide sur les smartphones de toutes marques, avec une **configuration minimale** précisée.
- **Protection des données** conforme au RGPD.

## Public visé et accessibilité

EMOTISM s'adresse à des personnes **diagnostiquées TSA** et vise à être recommandé par des professionnels de santé **en complément d'un suivi**. Il n'est pas conçu pour le grand public, et il ne remplace pas un traitement.

Le joueur doit avoir au moins 7 ans et un minimum de 3 ans de socialisation pour pouvoir relier les situations du jeu à la vie réelle.

L'interface est pensée pour le **confort cognitif** : textes et icônes clairs, **limitation de la surcharge sensorielle** (visuelle et sonore).

## Installation

### Prérequis

- **Unity** : `À COMPLÉTER` (version exacte, ex. `6000.0.xx`). Installer via [Unity Hub](https://unity.com/download) avec le module de build de la plateforme visée (Android / iOS).
- **Git**

### Ouvrir le projet

```bash
git clone https://github.com/Janou023/Projet_Emotism.git
```

1. Ouvrir **Unity Hub** > **Add** > **Add project from disk**.
2. Sélectionner le dossier **`Projet_Unity`** (et non la racine du dépôt).
3. Ouvrir avec la version d'Unity indiquée ci-dessus.



## Organisation du dépôt

```
Projet_Emotism/
├── Projet_Unity/   # Projet Unity (Assets, Packages, ProjectSettings)
└── README.md
```

## Calendrier

Le projet dure un semestre, du **24 septembre au 17 décembre 2026**.

| Date | Jalon |
|------|-------|
| 02/10 | Cahier des charges |
| 08/10 | Rapport de recherche (état de l'art) et rapport d'entretiens avec les experts |
| 04/11 | Prototype et **soutenance intermédiaire** |
| 05/12 | Produit pour les tests utilisateurs et rapport de résultats |
| 15/12 | Produit final |
| 17/12 | **Soutenance finale** |
| 18/12 | Rapport technique synthétique |

## Équipe

Encadrement : **Noémie Chaniaud** et **Edwige Clermont**.

| 1A | 2A |
|----|----|
| Fleur Affolter-Cauvin | Jan De Graeve |
| Alex Fioretzi--Ysabel | Paolo Kehl |
| Caroline Promis | Zoé Schliefer Berlioux |
| Léna Serin | Ambre Souchon--Becharel |

## Contribuer

Le travail est organisé avec les **Issues** et le **Project** de ce dépôt.

1. Choisir une issue et se l'assigner.
2. Créer une branche : `feature/nom-court` ou `fix/nom-court`.
3. Ouvrir une **Pull Request** liée à l'issue (`Closes #numéro`).
4. Attendre une relecture avant de fusionner dans `main`.
