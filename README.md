<<<<<<< HEAD
# Projekt-gr.2-Money-Mystery-Murder








## Baza danych i backend
Projekt używa MySQL jako backend do przechowywania statystyk graczy.

Struktura bazy
- Baza: `mmm`
- Tabela: `players`  

Utworzenie bazy i tabeli
1. Otwórz phpMyAdmin lub terminal MySQL.  
2. Wykonaj skrypt `CreatePlayersTable.sql`:

```sql
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

Uruchom backend:
    dotnet restore
    dotnet run

Backend działa na http://localhost:5100
Endpointy: GET /players, POST /players
=======
# Projekt-gr.2-Money-Mystery-Murder
>>>>>>> c41e47bb5f2e86e1a0411a514d0da0bb22320e71
