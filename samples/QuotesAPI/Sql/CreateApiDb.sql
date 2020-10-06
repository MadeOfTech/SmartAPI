CREATE TABLE dbtype (
	id INTEGER PRIMARY KEY AUTOINCREMENT,
    designation varchar(250) NOT NULL,
    CONSTRAINT ux_dbtype_designation UNIQUE (designation)
);

CREATE TABLE db (
	id INTEGER PRIMARY KEY AUTOINCREMENT,
	designation VARCHAR(250) NOT NULL,
	dbtype_id INT NOT NULL,
	connectionstring VARCHAR(250) NOT NULL,
    CONSTRAINT fk_db__dbtype FOREIGN KEY (dbtype_id) REFERENCES dbtype (id)
);

CREATE TABLE api (
	id INTEGER PRIMARY KEY AUTOINCREMENT,
    designation VARCHAR(250) NOT NULL,
    description VARCHAR(250) NOT NULL,
    basepath VARCHAR(250) NOT NULL DEFAULT (''),
    CONSTRAINT ux_api_designation UNIQUE (designation)
);

CREATE TABLE collection (
	id INTEGER PRIMARY KEY AUTOINCREMENT,
    api_id INT NOT NULL,
    collectionname varchar(250) NOT NULL,
    membername varchar(250) NOT NULL,
    db_id INT NOT NULL,
	tablename VARCHAR(250) NOT NULL,
    description TEXT NULL,
    publish_getcollection BIT NOT NULL DEFAULT(1),
    publish_getmember BIT NOT NULL DEFAULT(1),
    publish_postmember BIT NOT NULL DEFAULT(0),
    publish_putmember BIT NOT NULL DEFAULT(0),
    publish_deletemember BIT NOT NULL DEFAULT(0),
    CONSTRAINT fk_collection__api FOREIGN KEY (api_id) REFERENCES api (id),
    CONSTRAINT ux_collection_api_id_collectionname UNIQUE (api_id, collectionname)
);

CREATE TABLE attribute (
	id INTEGER PRIMARY KEY AUTOINCREMENT,
	collection_id INT NOT NULL,
	attributename VARCHAR(128) NOT NULL,
	columnname VARCHAR(128) NOT NULL,
    description TEXT NULL,
	type VARCHAR(10) NOT NULL,
    format VARCHAR(10) NULL,
    autovalue BIT(1) NOT NULL DEFAULT(0),
    nullable BIT(1) NOT NULL DEFAULT(0),
    keyindex INT NULL,
    fiqlkeyindex INT NULL,
    CONSTRAINT fk_attribute__collection FOREIGN KEY (collection_id) REFERENCES collection (id),
    CONSTRAINT ux_attribute_collection_id_attributename UNIQUE (collection_id, attributename),
    CONSTRAINT ux_attribute_collection_id_keyindex UNIQUE (collection_id, keyindex)
);

INSERT INTO dbtype (designation) VALUES ('mysql'),('sqlite');

INSERT INTO db (designation, dbtype_id, connectionstring) SELECT
'quotesdb', id, 'Data Source=quotesdb.sqlite'
FROM dbtype
WHERE designation='sqlite';

INSERT INTO api (designation, description, basepath)
VALUES ('Quotes API v1', 'API that exposes popular quotes', '/quotesapi/v1');

INSERT INTO collection (api_id, collectionname, membername, db_id, tablename, publish_getcollection, publish_getmember, publish_postmember, publish_putmember, publish_deletemember)
SELECT api.id, 'quotes', 'quote', db.id, 'quote', 1, 1, 1, 1, 1
FROM db, api
WHERE db.designation = 'quotesdb' AND api.designation = 'Quotes API v1';

INSERT INTO collection (api_id, collectionname, membername, db_id, tablename, publish_getcollection, publish_getmember, publish_postmember, publish_putmember, publish_deletemember)
SELECT api.id, 'authors', 'author', db.id, 'author', 1, 1, 1, 1, 1
FROM db, api
WHERE db.designation = 'quotesdb' AND api.designation = 'Quotes API v1';

INSERT INTO collection (api_id, collectionname, membername, db_id, tablename, publish_getcollection, publish_getmember, publish_postmember, publish_putmember, publish_deletemember)
SELECT api.id, 'detailed_quotes', 'detailed_quote', db.id, 'v_quote', 1, 1, 0, 0, 0
FROM db, api
WHERE db.designation = 'quotesdb' AND api.designation = 'Quotes API v1';


INSERT INTO attribute (collection_id, attributename, columnname, type, format, autovalue, keyindex)
SELECT id, 'id', 'id', 'integer', 'int32', 1, 0
FROM collection WHERE tablename='quote';

INSERT INTO attribute (collection_id, attributename, columnname, type, format)
SELECT id, 'author_id', 'author_id', 'integer', 'int32'
FROM collection WHERE  tablename='quote';

INSERT INTO attribute (collection_id, attributename, columnname, type, format)
SELECT id, 'message', 'message', 'string', NULL
FROM collection WHERE  tablename='quote';


INSERT INTO attribute (collection_id, attributename, columnname, type, format, autovalue, keyindex)
SELECT id, 'id', 'id', 'integer', 'int32', 1, 0
FROM collection WHERE tablename='author';

INSERT INTO attribute (collection_id, attributename, columnname, type)
SELECT id, 'name', 'name', 'string'
FROM collection WHERE  tablename='author';


INSERT INTO attribute (collection_id, attributename, columnname, type, format, autovalue, keyindex, fiqlkeyindex)
SELECT id, 'id', 'id', 'integer', 'int32', 1, 0, 1
FROM collection WHERE tablename='v_quote';

INSERT INTO attribute (collection_id, attributename, columnname, type, fiqlkeyindex)
SELECT id, 'author_name', 'author_name', 'string', 2
FROM collection WHERE  tablename='v_quote';

INSERT INTO attribute (collection_id, attributename, columnname, type, format, fiqlkeyindex)
SELECT id, 'message', 'message', 'string', NULL, 3
FROM collection WHERE  tablename='v_quote';