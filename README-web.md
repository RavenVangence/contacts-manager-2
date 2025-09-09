# Web Migration (ASP.NET Core + React)

This folder adds a new Web API in `Server/` and a React (Vite) client in `client/` to migrate from the original WPF app.

## Structure
- `Server/` ASP.NET Core Web API (in-memory contacts list).
- `client/` React UI (Vite dev server, builds to static assets you can later copy into `Server/wwwroot`).

## Dev Run
1. Install client deps:
```
cd client
npm install
npm run dev
```
2. Run API:
```
cd ../Server
# (Add project to solution first if needed)
# dotnet run
```
3. Visit React dev server (calls API via `VITE_API` env var if set).

Create `.env` inside `client` with:
```
VITE_API=http://localhost:5000
```
Adjust port to actual API port.

## Build + Serve Static
```
cd client
npm run build
# copy dist/* into Server/wwwroot (create if missing) then: dotnet run
```

## Publish Single File
```
dotnet publish Server/ContactsManager.Server.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true
```

## Next Steps
- Add persistence (EF Core + SQLite).
- Add validation parity (Regex, length) on both server + client.
- Replace in-memory repo with DbContext.
