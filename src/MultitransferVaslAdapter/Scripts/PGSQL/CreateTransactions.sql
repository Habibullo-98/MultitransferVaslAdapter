CREATE TABLE transactions
(
    transaction_id UUID PRIMARY KEY,

    record_type VARCHAR(32) NOT NULL,
    service_id UUID NULL,

    country_code VARCHAR(8) NOT NULL,

    status VARCHAR(32) NOT NULL,
    external_transaction_num VARCHAR(64) NOT NULL,

    created_at_utc TIMESTAMP WITH TIME ZONE NOT NULL,
    updated_at_utc TIMESTAMP WITH TIME ZONE NOT NULL,

    beneficiary_account_number VARCHAR(128) NULL,
    beneficiary_phone_number VARCHAR(32) NULL,
    sender_comment VARCHAR(256) NULL,

    raw_request_json TEXT NOT NULL,

    last_error_code INTEGER NOT NULL,
    last_error_external VARCHAR(256) NULL
);