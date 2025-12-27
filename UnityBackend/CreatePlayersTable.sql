CREATE DATABASE IF NOT EXISTS mmm;

USE mmm;

CREATE TABLE IF NOT EXISTS players (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(50),
    Kills INT,
    Money BIGINT,
    Wins INT,
    TotalGames INT,
    WinRate FLOAT
);