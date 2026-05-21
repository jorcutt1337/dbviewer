connection strings go in app.config

db schema saved after 1st time loading app to "bin\Debug\net10.0-windows\DB_SCHEMA.xml". 
to have app re-generate this file (refresh schema info), just delete it (DB_SCHEMA.xml).

auto-generate query joins initial checkin supports only relational (fk) databases. 

had oracle support and auto-gen implicit query joins in another version, todo
