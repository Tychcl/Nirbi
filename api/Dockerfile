FROM php:8.2-apache

# Установка системных зависимостей
RUN apt-get update && apt-get install -y \
    libzip-dev \
    libpng-dev \
    libjpeg-dev \
    libfreetype6-dev \
    libssl-dev \
    unzip \
    git \
    && docker-php-ext-configure gd --with-freetype --with-jpeg \
    && docker-php-ext-install -j$(nproc) gd zip pdo_mysql sockets bcmath

# Включение mod_rewrite в Apache
RUN a2enmod rewrite

# Установка Composer
COPY --from=composer:latest /usr/bin/composer /usr/bin/composer

# Установка рабочей директории
WORKDIR /var/www/html

# Копирование composer файлов сначала для кеширования зависимостей
COPY composer.json composer.lock ./

# Установка зависимостей Composer
RUN composer install --no-dev --optimize-autoloader

# Копирование конфига Apache (теперь из той же папки api)
COPY configs/apache/000-default.conf /etc/apache2/sites-available/000-default.conf

# Копирование остальных файлов проекта
COPY . .

# Установка прав на запись
RUN chown -R www-data:www-data /var/www/html

EXPOSE 80