CREATE TABLE IF NOT EXISTS hash_values (
    sha256 CHAR(64) PRIMARY KEY,
    md5 CHAR(32) NOT NULL,
    sha1 CHAR(40) NOT NULL,
    sha512 CHAR(128) NOT NULL
);

CREATE TABLE IF NOT EXISTS file_multi_analyses
(
    file_multi_analysis_id uuid PRIMARY KEY,
    is_private boolean NOT NULL,
    started_date timestamp with time zone NOT NULL,
    status smallint NOT NULL,
    verdict smallint NOT NULL,
    threat_zone smallint NOT NULL,
    average_threat_score real,
    file_name varchar(100) NOT NULL,
    size bigint NOT NULL,
    content_type text NOT NULL,
    user_id varchar(450) REFERENCES "AspNetUsers" ("Id"),
    sha256 char(64) REFERENCES hash_values(sha256) ON DELETE CASCADE
);

CREATE TABLE file_service_analyses (
    file_service_analysis_id varchar(200) PRIMARY KEY,
    service_name varchar(30) NOT NULL,
    status smallint NOT NULL,
    verdict smallint NOT NULL,
    threat_zone smallint NOT NULL,
    raw_threat_score real,
    max_possible_threat_score real
);

CREATE TABLE IF NOT EXISTS file_analyses
(
    multi_analysis_id uuid REFERENCES file_multi_analyses(file_multi_analysis_id) ON DELETE RESTRICT,
    service_analysis_id varchar(200) REFERENCES file_service_analyses (file_service_analysis_id) ON DELETE RESTRICT,
    CONSTRAINT PK_file_analysis PRIMARY KEY (multi_analysis_id, service_analysis_id)
);

CREATE TABLE reports (
    report_id VARCHAR(200) PRIMARY KEY,
    verdict smallint NOT NULL,
    threat_zone smallint NOT NULL,
    raw_threat_score real,
    max_possible_threat_score real,
    file_service_analysis_id VARCHAR(200) NOT NULL REFERENCES file_service_analyses (file_service_analysis_id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS url_multi_analyses
(
    url_multi_analysis_id uuid PRIMARY KEY,
    is_private boolean NOT NULL,
    started_date timestamp with time zone NOT NULL,
    status smallint NOT NULL,
    verdict smallint NOT NULL,
    threat_zone smallint NOT NULL,
    average_threat_score real,
    url varchar(2083) NOT NULl,
    user_id varchar(450) NOT NULL REFERENCES "AspNetUsers"("Id"),
    sha256 char(64) NOT NULL REFERENCES hash_values(sha256)
);

CREATE TABLE url_service_analyses (
    url_service_analysis_id varchar(200) PRIMARY KEY,
    service_name varchar(30) NOT NULL,
    status smallint NOT NULL,
    verdict smallint NOT NULL,
    threat_zone smallint NOT NULL,
    raw_threat_score real,
    max_possible_threat_score real
);

CREATE TABLE IF NOT EXISTS url_analyses
(
    multi_analysis_id uuid REFERENCES url_multi_analyses(url_multi_analysis_id),
    service_analysis_id varchar(200) REFERENCES url_service_analyses(url_service_analysis_id),
    CONSTRAINT PK_url_analyses PRIMARY KEY (multi_analysis_id, service_analysis_id)
);

CREATE TABLE IF NOT EXISTS email_address_multi_reputations (
    email_address_multi_reputation_id uuid PRIMARY KEY,
    evaluation_date timestamp with time zone NOT NULL,
    final_verdict smallint NOT NULL,
    final_threat_zone smallint NOT NULL,
    email_address varchar(254) NOT NULL
);

CREATE TABLE IF NOT EXISTS email_address_service_reputations (
    email_address_service_reputation_id uuid PRIMARY KEY,
    service_name varchar(30) NOT NULL,
    verdict smallint NOT NULL,
    threat_zone smallint NOT NULL,
    is_disposable boolean,
    is_risky_tld boolean,
    email_address_multi_reputation_id uuid REFERENCES email_address_multi_reputations (email_address_multi_reputation_id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS phone_multi_reputations
(
    phone_multi_reputation_id uuid PRIMARY KEY,
    evaluation_date timestamp with time zone NOT NULL,
    final_verdict smallint NOT NULL,
    final_threat_zone smallint NOT NULL,
    phone_number varchar(16) NOT NULL
);

CREATE TABLE IF NOT EXISTS phone_services_reputations
(
    phone_service_reputation_id uuid PRIMARY KEY,
    service_name varchar(30) NOT NULL,
    verdict smallint NOT NULL,
    threat_zone smallint NOT NULL,
    phone_local_format varchar(30) NOT NULL,
    phone_country_code char(2) NOT NULL,
    phone_dialing_code smallint NOT NULL,
    phone_line_type varchar(20) NOT NULL,
    phone_multi_reputation_id uuid REFERENCES phone_multi_reputations(phone_multi_reputation_id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS message_analysis (
    message_analysis_id uuid PRIMARY KEY,
    is_private boolean NOT NULL,
    started_date timestamp with time zone NOT NULL,
    message_type smallint NOT NULL,
    message_sender varchar(254) NOT NULL,
    message_subject varchar(998),
    message_content text NOT NULL,
    status smallint NOT NULL,
    verdict smallint NOT NULL,
    threat_zone smallint NOT NULL,
    user_id varchar(450) NOT NULL REFERENCES "AspNetUsers" ("Id"),
    sha256 char(64) NOT NULL REFERENCES hash_values (sha256) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS attached_file_results (
    attached_file_result_id uuid PRIMARY KEY,
    data_type smallint NOT NULL,
    file_name varchar(100) NOT NULL,
    size bigint NOT NULL,
    content_type text NOT NULL,
    message_analysis_id uuid NOT NULL REFERENCES message_analysis (message_analysis_id) ON DELETE RESTRICT,
    file_multi_analysis_id uuid NOT NULL REFERENCES file_multi_analyses (file_multi_analysis_id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS detected_url_results (
    detected_url_results_id uuid PRIMARY KEY,
    data_type smallint NOT NULL,
    url varchar(2083) NOT NULL,
    message_analysis_id uuid NOT NULL REFERENCES message_analysis (message_analysis_id) ON DELETE RESTRICT,
    url_multi_analysis_id uuid NOT NULL REFERENCES url_multi_analyses (url_multi_analysis_id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS detected_email_address_results (
    detected_email_address_results_id uuid PRIMARY KEY,
    data_type smallint NOT NULL,
    email_address varchar(254) NOT NULL,
    message_analysis_id uuid NOT NULL REFERENCES message_analysis (message_analysis_id) ON DELETE RESTRICT,
    email_address_multi_reputation_id uuid NOT NULL REFERENCES email_address_multi_reputations (email_address_multi_reputation_id) ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS detected_phone_number_results (
    detected_phone_number_results_id uuid PRIMARY KEY,
    data_type smallint NOT NULL,
    phone_number varchar(16) NOT NULL,
    message_analysis_id uuid NOT NULL REFERENCES message_analysis (message_analysis_id) ON DELETE RESTRICT,
    phone_multi_reputation_id uuid NOT NULL REFERENCES phone_multi_reputations (phone_multi_reputation_id) ON DELETE RESTRICT
);