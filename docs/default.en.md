---
title: 'SmartAPI'
template: item
---

# Have you ever wondered?

Have you ever thought to yourself that the code of all APIs is furiously alike?
Whether you spend more time copying and pasting to transform HTTP queries
to SQL requests? That everyone who contributes to the project will write the content
of the OpenAPI documentation in a different way?

If the answer to each of these questions is yes, the following should interest you!

# Introduction

## Purpose

This project is a middleware helping people to build automatically, a complete
RESTful API and its documentation, based upon database structure. This way,
CRUD method will be generated to map operation to database command. This approach
offer consistency in API developpement, eases the maintenance and evolution and
moreover, decreases by a big factor, bugs.

## Why ?

It's been a few years now that in the different companies where I worked, we
published tons af API. From my point of view, we've never made any effort to
be time efficient... and especially since the advent of REST.

When we were using SOAP, API specifications could always be weird :
idempotency was a concept, not a rule, we were publishing methods
and the way we were implementing actions could be esoteric. But since REST,
things have changed.

REST is a fantastic approach since operations described are simple :
create, read, update and delete. All of these verbs are more or less related
to the traditionnal vocabulary of SQL, INSERT, SELECT, UPDATE and DELETE.
More or less only, since CREATE can be done thru POST and PUT verbs. But,
let's get optimistic : why continue to write code that take a REST request to
transform it into a SQL request ? From my point of view, it was sufficient
to automate database access based upon verb and ressource used in the request.

Automation could also be the opportunity to presents every API the same way,
to have an exhaustive documentation about objects, arrays and error codes,
a generic approach to collections and members, and a way to manage json and
xml without any more line of code. But overall, it could be the opportunity
to implement selection on collections, based upon a simple query langage
named FiQL. Automate could be the opportunity to do all of this without any
code, effort, nor complexity.

And, i did it ? You want to know how ? Let's continue !

# Technical overview

## Technical considerations about REST

Considering Wikipedia definition of REST, there's an
[historical method semantics]
(https://en.wikipedia.org/wiki/Representational_state_transfer#Discussion) for
each HTTP method :

HTTP METHODS | Collection resource, such as https://api.example.com/collection | Member resource, such as https://api.example.com/collection/item3
------------ | --------------------------------------------------------------- | -----------------------------------------------------------------
GET          | Retrieve the URIs of the member resources of the collection resource in the response body. | Retrieve representation of the member resource in the response body.
POST         | Create a member resource in the collection resource using the instructions in the request body. The URI of the created member resource is automatically assigned and returned in the response Location header field. | Create a member resource in the member resource using the instructions in the request body. The URI of the created member resource is automatically assigned and returned in the response Location header field.
PUT          | Replace all the representations of the member resources of the collection resource with the representation in the request body, or create the collection resource if it does not exist. | Replace all the representations of the member resource or create the member resource if it does not exist, with the representation in the request body.
PATCH        | Update all the representations of the member resources of the collection resource using the instructions in the request body, or may create the collection resource if it does not exist. | Update all the representations of the member resource, or may create the member resource if it does not exist, using the instructions in the request body.
DELETE       | Delete all the representations of the member resources of the collection resource. | Delete all the representations of the member resource.

In this middleware, we'll keep only obvious semantics :

HTTP METHODS | Collection resource, such as https://api.example.com/collection | Member resource, such as https://api.example.com/collection/item3
------------ | --------------------------------------------------------------- | -----------------------------------------------------------------
GET          | Retrieve the URIs of the member resources of the collection resource in the response body. | Retrieve representation of the member resource in the response body.
POST         | Create a member resource in the collection resource using the instructions in the request body. The URI of the created member resource is automatically assigned and returned in the response Location header field. |
PUT          | | Replace all the representations of the member resource or create the member resource if it does not exist, with the representation in the request body.
DELETE       | | Delete all the representations of the member resource.

Another way to consider kept methods is the following : scope of methods is only
member resource except for GET to retreive a complete collection. Moreover, PATCH
has been removed, due to the complexity of its usage (but sure, it will be
implemented one day).

## Swagger Datatypes

Known swagger types are the following :

type	| format	| Comments
--------|-----------|----------------------------------
integer	| int32     | signed 32 bits
integer	| int64     | signed 64 bits (a.k.a long)
number	| float	    | 
number	| double	| 
string	| 	        | 
string	| byte	    | base64 encoded characters
string	| binary	| any sequence of octets
boolean	| 	        | 
string	| date      | As defined by full-date - RFC3339
string	| date-time	| As defined by date-time - RFC3339
string	| password	| A hint to UIs to obscure input.

As a consequence, any attribute defined by a tuple (type, format) that won't
conform to a definition exposed in this table won't be considered as a valid
attribute.

Moreover, for convenience in early versions of this lib, the following 
tuple (type, format) will be managed the following way :

type	| format	| input / output
--------|-----------|--------------------------------------------------------------
number	| float	    | \d+.\d+ (no engineering notation with exponents, for example)
number	| double	| \d+.\d+ (no engineering notation with exponents, for example)
string	| binary	| 0x[[0-9a-fA-F]{2}]* / 0x[[0-9A-F]{2}]*
boolean	| 	        | ([Tt][Rr][Uu][Ee]|[Ff][Aa][Ll][Ss][Ee]) / true | false
string	| byte      | Not supported for instance
string	| date      | Not supported for instance
string	| date-time	| see date-time input format / "yyyyMMddTHHmmssfffZ"
string	| password	| Not supported for instance

date-time input format are the following :

```
// Basic formats
"yyyyMMddTHHmmssfffzzz"
"yyyyMMddTHHmmssfffzz"
"yyyyMMddTHHmmssfffZ"
// Extended formats
"yyyy-MM-ddTHH:mm:ss.fffzzz"
"yyyy-MM-ddTHH:mm:ss.fffzz"
"yyyy-MM-ddTHH:mm:ss.fffZ"
// Basic formats
"yyyyMMddTHHmmsszzz"
"yyyyMMddTHHmmsszz"
"yyyyMMddTHHmmssZ"
// Extended formats
"yyyy-MM-ddTHH:mm:sszzz"
"yyyy-MM-ddTHH:mm:sszz"
"yyyy-MM-ddTHH:mm:ssZ"
```

All date time all internally manipulated as `DateTimeKind.Utc`.

## Generated SQL requests

As far as collections are a mapping of a table or a view, generated SQL requests
are very simple and then, efficient... except for collections request.

Attention must be paid to the way you'll communicate with your clients. If exposed
tends to be huge, a good idea would be to expose views so that data is keept
reasonnably small and request efficient regarding to the keys.

## Share storage, not data

Considering SaaS development, major stakes are performances and tenants isolations.
The way i've done this, till the beginning of my carrer, was to inject in all of
requests a clause to verify that the user is allowed to access data. For exemple,
if users are idenfied by member_id, and that our data are stored in a table called
note, request will be : `SELECT * FROM note WHERE (member_id=666)`.

Now, let's imagine that a new table related to note by not directly to member. For
example, a table called picture, to store picture related to a note :

```
CREATE TABLE picture (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    note_id INT NOT NULL,
    title VARCHAR(250) NOT NULL,
    data BLOB NOT NULL,
    CONSTRAINT fk_picture__note FOREIGN KEY (note_id) REFERENCES note (id),
    CONSTRAINT ux_picture_title UNIQUE (note_id, title)
)
```

No reference to member_id is done. From a strict SQL point of view, it complies
with bast practices. In other word, introduce member_id in picture would violate
the second normal form. But, there's another way to conciliate the 2FN and the
necessety to verify that user is allowed to access the ressource : a view.

```
CREATE VIEW api_picture AS
SELECT member_id, picture.id AS id, note_id, picture.title AS title, data FROM picture INNER JOIN note ON note.id=picture.note_id
```

For GET verbs, this is done ! What about other verbs ? Let's use triggers :

```
CREATE TRIGGER tr_insteadofdelete_api_picture INSTEAD OF DELETE ON api_picture
FOR EACH ROW
BEGIN
    DELETE FROM picture WHERE id IN (
        SELECT picture.id FROM picture INNER JOIN note ON note.id=picture.note_id AND note.member_id=OLD.member_id
	) AND id=OLD.id;
END
```

That's all folks. Integrity of actions is kept in the database, and not in the code.
Please note that the view + instead of triggers approach works well with MySQL,
SQL Server, and probably other SQL engine, but can't work with sqlite. The reason
is the following :

> If an INSERT occurs within a trigger then this routine will return the rowid of
> the inserted row as long as the trigger is running. Once the trigger program ends,
> the value returned by this routine reverts to what it was before the trigger was
> fired.
>
> -- *https://sqlite.org/c3ref/last_insert_rowid.html*

In other words, every post requests will return a wrong location if they use
a instead of trigger with sqlite.

As a conclusion, injection is a systematic ways to deals with shared storage
for dedicated data. And its very robust as far as requests will failed if
the table (or view) you try to access to doesn't contain your injected attribute.

How to inject your value ? This is done by defining 2 options in the SmartAPIOptions :

* `InjectAttribute_Name` : this is the column name that every tables have to exposes
to be used with the API. Every SQL request will be injected with this parameter ;
* `InjectAttribute_ValueEvaluator`  : depending on the context and the aim of the
API, the value corresponding to the previous column can be computed in a defferent
way. This is the reason why this value is systematically evaluated. Every
developper will have the possibility to choose the best way to proceed to this
evaluation.

## A small arrangement with the standard

Among the options, there is `Upsert_FillBodyWithMember`.
If set to true, the API will behave a little differently from the
standard for upsert operations ie. POST and PUT. Indeed, in addition
to returning the recommended headers it will fill the body with member
representation. This behaviour will be reflected in the swagger documentation.

## Trigger_AfterOperation

This parameters allows you to trig actions based upon operation successfully
done. This is an example, taken from QuotesAPI :

```
options.Trigger_AfterOperation = async (context, collection, input, keys) =>
{
    if (context.Request.Method == "POST" && collection.collectionname == "authors")
    {
        // A new author has been inserted.
        string name = input["name"];
        Console.WriteLine("A new author has been inserted : " + name);
    }
};
```

The parameters are the following :

* `context` is an HttpContext, which give you many informations about the Http Call ;
* `collection` is the collection on which operation is applyed ;
* `input` is the object the caller sent into the body ;
* `keys` is a string array containing key values, enumerated in the order decribed into the database.

# Samples

2 samples are delivered with this solution : QuotesAPI and NotesAPI. Both of them
are designed to help developper to understand the way SmartAPI works. To facilitate
understanding, it's easier to use smaples in the following order :

* QuotesAPI
* NotesAPI

## QuotesAPI

This sample API is a open API to world in read mode, but is private for modifications.
To access data in modification, you'll have to authenticate as `admin`, with the
password `secret`. The authentication used in this sample is basic authentication.

The API exposes 2 tables and one view to illustrate the simplicity to join data
with the view to reduces requests.

It must be noted that FiQL requests are allowed to request detailed quotes :
this way, it's easy to search for quotes containing, for example, the word pepper :

/quotesapi/v1/detailed_quotes?query=message==*pepper*

This way, beware to performances (indexation) and volume of data (which can be
huge).

## NotesAPI

This sample API is user centric and exposed data that are stored in a common DB. The user
has to login to use API and to access to ressources : folders and notes. Basically,
notes are small messages with title and content that are stored in a folder.

Every ressources are the property of the loggued user, and none of them are shared
in any way with another user. It supposes to ensure that every requests are
related to the used that is loggued. To do that, Injection has been used.

The way this application is build is with 2 dbs : one for API configuration and one
for data. data includes member credentials. 2 members are available :

* `garlo`, whose password is `secret_of_garlo` ;
* `posegue`, whose password is `secret_of_posegue`.

The authentication used in this sample is basic authentication. This job is done
by `BasicAuthenticationHandler` that,moreover, complete claims for the principal
(`context.Principal`) with its member_id. This way, once the user is authenticated,
the `InjectAttribute_ValueEvaluator` will just have to look for this claim to
allow SmartAPI to complete injection.

# Author

* **Fabien Philippe** - *Initial work* - [GitHub](https://github.com/fphilippe), [LinkedIn](https://www.linkedin.com/in/fabienphilippe/)

# Special thanks

I warmly thank [API-K](https://www.api-k.com), and more particularly 
[Pascal Roux](https://www.linkedin.com/in/pascal-roux-6528a118) for its trust and
for the time he left me to complete this project!

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