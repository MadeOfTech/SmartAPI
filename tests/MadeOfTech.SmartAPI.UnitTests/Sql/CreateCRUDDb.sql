CREATE TABLE testentity_table (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    designation VARCHAR(250) NOT NULL,
    ts DATETIME NOT NULL
);

INSERT INTO testentity_table (designation, ts) VALUES
 ('test 1', '2020-01-01 10:10:00')
,('test 2', '2021-01-01 10:10:00');