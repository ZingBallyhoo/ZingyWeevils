# ZingyWeevils

Live instances:
- Bin Weevils: https://bw.zingy.dev
- Weevil World: https://ww.zingy.dev

----------------------

How to run:
- Update archive path in BinWeevils.Server/appsettings.Development.json
- `cd BinWeevils.Server`
- `dotnet run`

----------------------

Game TODO:
- Word search, cross words (missing data)
- Nest score calculation isn't the same
- Castle Gam room events
- Brain train leaderboard/mode scores
  - Also has missing audio files...
- Mulch-tastic
- Rotating shop content (or well, make the shop content better)
- Public Plazas
- Tycoon Earnings
- Cinema / Home Cinema
- Camera (no)
- Magazines 
  - missing data..
- Unlockable pet juggling tricks
- Vector art for more rooms. but I likely don't have the data...

Technical TODO:
- More integration testing, including gameplay
- Validate pet actions
- Support different cores (currently only 29/30)
- Deprecate "SmartFoxServer" component in favour of ProtoActor
  - TaskQueue has got to go...

Ruffle Todo:
- Graphics:
  - Line thickness is scaled instead of being in screen coordinates
  - 9-slice scaling / scale9Grid not implemented
  - Totem of the aztecs missing dial rotation graphic (other missions too i assume)
  - Tink's Blocks rotation gets stuck
- Sound:
  - Weevil Wheels (loud)
  - Gaining mulch (loud)
  - Lost Diamond - metal detector is hard to use
- Other
  - Panic on opeing external browser window
  - Moving objects don't cause the cursor to change
  - LocalConnection for populating username in menu bar

---- 

Notes
- Create a migration
  - `cd BinWeevils.Server`
  - `dotnet ef migrations add <name> --project ../BinWeevils.Common`