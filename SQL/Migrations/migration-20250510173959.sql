CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE TABLE packages (
        id text NOT NULL,
        image_generations_count integer NOT NULL,
        avatar_generations_count integer NOT NULL,
        CONSTRAINT "PK_packages" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE TABLE prompts (
        id text NOT NULL,
        name text NOT NULL,
        number_in_order integer NOT NULL,
        text varchar(8192) NOT NULL,
        CONSTRAINT "PK_prompts" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE TABLE products (
        id text NOT NULL,
        name varchar(128) NOT NULL,
        type varchar(32) NOT NULL,
        price numeric(18,2) NOT NULL,
        discount numeric(18,2) NOT NULL,
        is_active boolean NOT NULL,
        package_id text,
        CONSTRAINT "PK_products" PRIMARY KEY (id),
        CONSTRAINT "FK_products_packages_package_id" FOREIGN KEY (package_id) REFERENCES packages (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE TABLE avatars (
        id text NOT NULL,
        name varchar(64) NOT NULL,
        description varchar(8192) NOT NULL,
        os_lora_file_path varchar(256) NOT NULL,
        created_at timestamp with time zone NOT NULL,
        user_id text NOT NULL,
        CONSTRAINT "PK_avatars" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE TABLE users (
        id text NOT NULL,
        avatar_generations_count integer NOT NULL,
        image_generations_count integer NOT NULL,
        created_at timestamp with time zone NOT NULL,
        invited_by_id text,
        active_avatar_id text,
        CONSTRAINT "PK_users" PRIMARY KEY (id),
        CONSTRAINT "FK_users_avatars_active_avatar_id" FOREIGN KEY (active_avatar_id) REFERENCES avatars (id) ON DELETE SET NULL,
        CONSTRAINT "FK_users_users_invited_by_id" FOREIGN KEY (invited_by_id) REFERENCES users (id) ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE TABLE images (
        id text NOT NULL,
        os_file_path text NOT NULL,
        created_at timestamp with time zone NOT NULL,
        user_id text NOT NULL,
        CONSTRAINT "PK_images" PRIMARY KEY (id),
        CONSTRAINT "FK_images_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE TABLE jobs (
        id text NOT NULL,
        type varchar(32) NOT NULL,
        status varchar(32) NOT NULL,
        input jsonb NOT NULL,
        output jsonb,
        notes text,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        user_id text NOT NULL,
        CONSTRAINT "PK_jobs" PRIMARY KEY (id),
        CONSTRAINT "FK_jobs_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE TABLE promocodes (
        id text NOT NULL,
        code varchar(32) NOT NULL,
        used_by_user_id text,
        "ProductId" text NOT NULL,
        CONSTRAINT "PK_promocodes" PRIMARY KEY (id),
        CONSTRAINT "FK_promocodes_products_ProductId" FOREIGN KEY ("ProductId") REFERENCES products (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_promocodes_users_used_by_user_id" FOREIGN KEY (used_by_user_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE TABLE telegram_profiles (
        telegram_id bigint NOT NULL,
        chat_state varchar(32) NOT NULL,
        settings jsonb NOT NULL,
        state_data jsonb NOT NULL,
        user_id text NOT NULL,
        CONSTRAINT "PK_telegram_profiles" PRIMARY KEY (telegram_id),
        CONSTRAINT "FK_telegram_profiles_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE TABLE orders (
        id text NOT NULL,
        status varchar(16) NOT NULL,
        type varchar(16) NOT NULL,
        payment_data jsonb,
        notes text,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        user_id text NOT NULL,
        product_id text,
        promocode_id text,
        CONSTRAINT "PK_orders" PRIMARY KEY (id),
        CONSTRAINT "FK_orders_products_product_id" FOREIGN KEY (product_id) REFERENCES products (id) ON DELETE SET NULL,
        CONSTRAINT "FK_orders_promocodes_promocode_id" FOREIGN KEY (promocode_id) REFERENCES promocodes (id) ON DELETE SET NULL,
        CONSTRAINT "FK_orders_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    INSERT INTO packages (id, avatar_generations_count, image_generations_count)
    VALUES ('00S2G6N181WCX6781J4X8A98XX', 1, 0);
    INSERT INTO packages (id, avatar_generations_count, image_generations_count)
    VALUES ('00S2G6N181WDQQRE4RPZEJ02Y1', 0, 100);
    INSERT INTO packages (id, avatar_generations_count, image_generations_count)
    VALUES ('00S2G6N181WJHSBKZZ8SMSRWY4', 1, 20);
    INSERT INTO packages (id, avatar_generations_count, image_generations_count)
    VALUES ('00S2G6N181WKCATSV5TQTHKPY8', 1, 100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    INSERT INTO products (id, discount, is_active, name, package_id, price, type)
    VALUES ('00S2G6N181NJWTPSA06P0G9SBC', 1000.0, TRUE, '1 аватар', '00S2G6N181WCX6781J4X8A98XX', 499.0, 'Package');
    INSERT INTO products (id, discount, is_active, name, package_id, price, type)
    VALUES ('00S2G6N181NKPW9Z56RM6R0KBF', 1000.0, TRUE, '100 генераций', '00S2G6N181WDQQRE4RPZEJ02Y1', 999.0, 'Package');
    INSERT INTO products (id, discount, is_active, name, package_id, price, type)
    VALUES ('00S2G6N181NMHDV50CAJDFVDBK', 1000.0, TRUE, 'Специальное предложение (1 аватар, 20 генераций)', '00S2G6N181WJHSBKZZ8SMSRWY4', 399.0, 'TrialPackage');
    INSERT INTO products (id, discount, is_active, name, package_id, price, type)
    VALUES ('00S2G6N181NNBFCB3JWCK7J7BP', 1000.0, TRUE, 'Начальный пакет (1 аватар, 100 генераций)', '00S2G6N181WKCATSV5TQTHKPY8', 999.0, 'FirstPaymentPackage');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE INDEX "IX_avatars_user_id" ON avatars (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE INDEX "IX_images_user_id" ON images (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE INDEX "IX_jobs_user_id" ON jobs (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE INDEX "IX_orders_product_id" ON orders (product_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE UNIQUE INDEX "IX_orders_promocode_id" ON orders (promocode_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE INDEX "IX_orders_user_id" ON orders (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE UNIQUE INDEX "IX_products_package_id" ON products (package_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE UNIQUE INDEX "IX_promocodes_code" ON promocodes (code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE INDEX "IX_promocodes_ProductId" ON promocodes ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE INDEX "IX_promocodes_used_by_user_id" ON promocodes (used_by_user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE UNIQUE INDEX "IX_telegram_profiles_user_id" ON telegram_profiles (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE UNIQUE INDEX "IX_users_active_avatar_id" ON users (active_avatar_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    CREATE INDEX "IX_users_invited_by_id" ON users (invited_by_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    ALTER TABLE avatars ADD CONSTRAINT "FK_avatars_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250510143658_Initial') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20250510143658_Initial', '9.0.3');
    END IF;
END $EF$;
COMMIT;

