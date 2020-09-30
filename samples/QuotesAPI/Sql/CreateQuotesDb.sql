CREATE TABLE author (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name VARCHAR(250) NOT NULL,
    CONSTRAINT ux_author_name UNIQUE (name)
);

CREATE TABLE quote (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    author_id INT NOT NULL,
    message TEXT NOT NULL,
    CONSTRAINT ux_quote_message UNIQUE (message)
);

CREATE VIEW v_quote AS
SELECT
    quote.id,
    author.name AS author_name,
    message
FROM quote INNER JOIN author ON author.id=quote.author_id;

INSERT INTO author (name) VALUES
 ('Marilyn Monroe')
,('Oscar Wilde')
,('Albert Einstein')
,('Eric Pennamen');

INSERT INTO quote (message, author_id)
SELECT 'I m selfish, impatient and a little insecure. I make mistakes, I am out of control and at times hard to handle. But if you can t handle me at my worst, then you sure as hell don t deserve me at my best.', id
FROM author WHERE name='Marilyn Monroe';

INSERT INTO quote (message, author_id)
SELECT 'Be yourself; everyone else is already taken.', id
FROM author WHERE name='Oscar Wilde';

INSERT INTO quote (message, author_id)
SELECT 'Two things are infinite: the universe and human stupidity; and I m not sure about the universe.', id
FROM author WHERE name='Albert Einstein';

INSERT INTO quote (message, author_id)
SELECT 'Hot pepper is like the highway: you pay at the exit!', id
FROM author WHERE name='Eric Pennamen';