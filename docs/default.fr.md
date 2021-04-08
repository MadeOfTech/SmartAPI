---
title: 'Introduction'
---

# Ne vous êtes-vous jamais demandé ?

Ne vous êtes-vous jamais dit que le code de toutes les API se ressemblait furieusement ?
Que vous passiez plus de temps à faire du copier-coller pour transformer des requêtes
HTTP en requêtes SQL ? Que chaque personne qui contribura au projet rédigera le contenu
de la documentation OpenAPI d'une manière différente ?

Si à chacune de ces question, la réponse est oui, la suite devrait vous intéresser !

# Introduction

## Objectif

Ce projet est un middleware permettant la construction automatique d'une API
RESTful complète ainsi que de sa documentation, en se basant sur une structure
de base de données. Ce faisant, toutes les opérations CRUD génèreront des appels
en base de données. Cette approche offre de la consistence dans le développement
d'API, facilite la maintenance et les évolutions, et finalement, est génératrice
de beaucoup moins de bugs.

## Pourquoi ?

Depuis le temps que je travaille dans différentes sociétés, nous avons publié
nombre d'API. De mon point de vue, nous n'avons jamais fait aucun effort pour
être efficaces... Surtout depuis l'avènement de REST.

en effet, lorque nous utilisions SOAP, les spécifications d'API pouvaient paraitre
bizarres : l'idempotence était un concept et non une règle, nous publiions des
méthodes et la facon dont nous implémentions les actions pouvait être ésotérique.
Mais depuis RESt, les choses ont changé.

REST est une approche fantastique car les opérations exposées sont simples :
créer, lire, mettre à jour et supprimer. tous ces verbes sont plus ou moins 
relatifs au vocabulaire traditionnel de SQL, INSERT, SELECT, UPDATE et DELETE.
Plus ou moins seulement, étant donné que l'opération INSERT peut être effectuée
par un verbe POST ou un PUT. Mais restons optimistes : pourquoi continuer d'écrire
du code qui prend une requète REST pour la transformer en requête SQL ? De mon
point de vue, cette réflexion était suffisante pour automatiser l'accès à la 
base de données en me basant sur le verbe HTTP et la ressource utilisée dans la
requête.

L'automatisation pouvait être, de surcroit, une formidable opportunité pour présenter
toutes les API de la même façon, pour avoir une documentation exhaustive sur les
objets exposés, les tableaux et les codes d'erreur, une approche générique sur 
les collections et les members, et pour faire cohabiter json et xml sans modifier
une seule ligne de code. Mais surtout, c'était l'opportunité pour implémenter la
sélection sur les collections, en se basant sur le langage de requêtage simple
FiQL. L'automatisation était l'occasion de faire tout ceci sans code, sans effort
et sans complexité.

Et ça fonctionne ! Vous voulez savoir comment ? C'est parti !

# Samples

2 samples sont livrés dans la solution : QuotesAPI et NotesAPI. tous 2 sont construit
de sorte à aider le développeur à comprendre comment fonctionne smartAPI. Pour faciliter
la compréhension, il est plus aisé de découvrir les samples dans cet ordre :

* QuotesAPI
* NotesAPI

## QuotesAPI

Cette API de démonstration est une API ouverte au monde en mode lecture seule, mais
offre aussi la possibilité d'apporter des modifications en mode privé. Pour accéder
aux données en modification, il faudra s'authentifier en tant qu'`admin`, avec le
mot de passe `secret`. L'authentification utilisée pour ce sample est une basic
authentication.

Cette API expose 2 tables et une vue pour illustrer la simplicité à réaliser des
jointures sur les données afin de réduire les requêtes.

Il est à noter que les requètes FiQL sont permises pour requèter les detailed quotes :
ce faisant, il est facile de chercher les citations contenant, par exemple, le mot
pepper :

/quotesapi/v1/detailed_quotes?query=message==*pepper*

Attention toutefois aux performances (indexation) et au volume de données (qui peut
devenir énorme).

## NotesAPI

Cette API de démonstration est centrée utilisateur et expose des données stockées
dans une base commune. L'utilisaeur doit se connecter pour utiliser l'API et accéder
à ses ressources : folders et notes. Les notes sont de courts messages avec un titre
et un contenu stockées dans des folders.

Chaque ressource est rattachée à un utilisateur, et aucune d'entre elles n'est
partagée avec un autre utilisateur. Cela suppose de s'assurer que toutes les
requêtes sont bien cohérentes avec l'utilisateur qui est loggué. Pour ce faire,
l'API utilise l'`injection`.

Cette application repose sur 2 bases de données :  l'une pour stocker la configuration
de l'API et l'autre pour les données utilisateur. La base de données data contient
les credentials des membres. 2 membres sont disponibles :

* `garlo`, dont le mot de passe est `secret_of_garlo` ;
* `posegue`, dont le mot de passe est `secret_of_posegue`.

L'authentification utilisée est une authentification basique. Cette fonction est
remplie par `BasicAuthenticationHandler` qui, de plus, complète les claims de 
l'utilisateur principal avec son `member_id`. Ainsi, une fois le membre authentifié,
la fonction `InjectAttribute_ValueEvaluator` n'aura plus qu'à s'appuyer sur ce
claim pour permettre à SmartAPI de compléter l'injection.

# Auteur

* **Fabien Philippe** - *Initial work* - [GitHub](https://github.com/fphilippe),
[LinkedIn](https://www.linkedin.com/in/fabienphilippe/)

# Special thanks

Je remercie chaleureusement [API-K](https://www.api-k.com), et plus particulièrement 
[Pascal Roux](https://www.linkedin.com/in/pascal-roux-6528a118) pour sa confiance
et pour le temps qu'il m'a laissé pour aboutir ce projet !

# License

Copyright 2020 Fabien Philippe

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   [http://www.apache.org/licenses/LICENSE-2.0](http://www.apache.org/licenses/LICENSE-2.0)

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.