ALTER TABLE transactions
ALTER COLUMN created_at_utc SET DEFAULT NOW();

ALTER TABLE transactions
ALTER COLUMN updated_at_utc SET DEFAULT NOW();