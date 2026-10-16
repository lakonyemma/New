-- Run as a MySQL administrator on MySQL 8.0+.
CREATE DATABASE IF NOT EXISTS `WorkwiseDB`
  CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
-- Create a dedicated application account and grant permissions to WorkwiseDB;
-- manage its password in the hosting secret store, NOT in committed files.
-- ASP.NET Core MVC uses EF Core EnsureCreated on an EMPTY WorkwiseDB.
-- Do not use this initializer as a schema migration tool on existing databases.
