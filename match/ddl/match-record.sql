BEGIN;

DO $$ BEGIN
    CREATE TYPE hash_type AS ENUM ('ldshash');
EXCEPTION
    WHEN duplicate_object THEN
        RAISE NOTICE 'hash_type ENUM already exists, skipping';
END $$;

DO $$ BEGIN
    CREATE TYPE status AS ENUM ('open', 'closed');
EXCEPTION
    WHEN duplicate_object THEN
        RAISE NOTICE 'status ENUM already exists, skipping';
END $$;

CREATE TABLE IF NOT EXISTS matches(
    id serial PRIMARY KEY,
    match_id text UNIQUE NOT NULL,
    created_at timestamp NOT NULL,
    initiator text NOT NULL,
    states text[2] NOT NULL,
    hash text NOT NULL,
    hash_type hash_type NOT NULL default 'ldshash',
    input jsonb,
    data jsonb NOT NULL,
    invalid bool NOT NULL default FALSE,
    status status NOT NULL default 'open'
);

COMMENT ON TABLE matches IS 'Match records';
COMMENT ON COLUMN matches.match_id IS 'Match record''s human-readable unique identifier.';
COMMENT ON COLUMN matches.created_at IS 'Match record''s creation date/time.';
COMMENT ON COLUMN matches.initiator IS 'Match record''s initiating entity.';
COMMENT ON COLUMN matches.states IS 'State/territory pair involved in match.';
COMMENT ON COLUMN matches.hash IS 'Value of hash used to identify match.';
COMMENT ON COLUMN matches.hash_type IS 'Type of hash used to identify match.';
COMMENT ON COLUMN matches.input IS 'Incoming data from real-time match request.';
COMMENT ON COLUMN matches.data IS 'Response data from match request.';
COMMENT ON COLUMN matches.invalid IS 'Indicator used for designating match as invalid.';
COMMENT ON COLUMN matches.status IS 'Match record''s status.';

COMMIT;
