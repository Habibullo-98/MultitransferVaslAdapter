CREATE INDEX ix_transactions_external_transaction_num
ON transactions(external_transaction_num);

CREATE INDEX ix_transactions_status
ON transactions(status);

CREATE INDEX ix_transactions_service_id
ON transactions(service_id);

CREATE INDEX ix_transactions_created_at_utc
ON transactions(created_at_utc);