CREATE TABLE IF NOT EXISTS payments (
  id varchar(64) NOT NULL,

  card_number varchar(16) NOT NULL,
  expiry varchar(5) NOT NULL,
  cvv varchar(3) NOT NULL,
  card_holder varchar(250) NULL,

  amount numeric(20, 6) NOT NULL,
  currency varchar(3) NOT NULL,

  merchant_id varchar(64) NOT NULL,

  status smallint NOT NULL,
  reason varchar(255) NULL,

  created_at timestamp with time zone NOT NULL DEFAULT NOW(),

  PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS outbox (
    id BIGSERIAL PRIMARY KEY,
    topic varchar(512) NOT NULL,
    key varchar(64) NOT NULL,
    data varchar(1048576) NOT NULL -- 1MB
);
