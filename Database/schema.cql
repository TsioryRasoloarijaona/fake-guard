-- FakeGuard -- Cassandra Schema
-- Keyspace : fake_news_ks
-- Maintenu par : responsable Cassandra DB Layer

CREATE KEYSPACE IF NOT EXISTS fake_news_ks
WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1};

USE fake_news_ks;

-- Table principale : lookup par article_id
CREATE TABLE IF NOT EXISTS articles (
    article_id       UUID,
    title            TEXT,
    content          TEXT,
    source_url       TEXT,
    submitted_at     TIMESTAMP,
    verdict          TEXT,
    confidence_score DOUBLE,
    PRIMARY KEY (article_id)
);

-- Table historique : triee par date DESC, partitionnee par mois
CREATE TABLE IF NOT EXISTS articles_by_date (
    bucket           TEXT,
    submitted_at     TIMESTAMP,
    article_id       UUID,
    title            TEXT,
    source_url       TEXT,
    verdict          TEXT,
    confidence_score DOUBLE,
    PRIMARY KEY (bucket, submitted_at, article_id)
) WITH CLUSTERING ORDER BY (submitted_at DESC, article_id ASC);

-- Table audit : historique complet des analyses par article
CREATE TABLE IF NOT EXISTS detection_results (
    article_id  UUID,
    analyzed_at TIMESTAMP,
    result_id   UUID,
    model_name  TEXT,
    verdict     TEXT,
    confidence  DOUBLE,
    PRIMARY KEY (article_id, analyzed_at)
) WITH CLUSTERING ORDER BY (analyzed_at DESC);

-- Table domaines blacklistes : lookup instantane par domaine
CREATE TABLE IF NOT EXISTS blacklisted_domains (
    domain        TEXT,
    label         TEXT,
    source        TEXT,
    last_update   TEXT,
    type          TEXT,
    harm_score    INT,
    accuracy      INT,
    transparency  INT,
    PRIMARY KEY (domain)
);
