# ✒️ Ink
Yet another minecraft server impementation. (in C#!)

![Screenshot of entity sync](https://github.com/GasInfinity/Ink/blob/main/docs/images/ecs-sync.png?raw=true)

📖 Little history about this:
  - I started this in summer of 2023 after I turned 17 just for fun, inspired by another C# server implementation [Obsidian](https://github.com/ObsidianMC/Obsidian) and a "library" to make minecraft servers [Minestom](https://minestom.net/). I thought I could implement a simple server with just Kestrel sockets and Pipelines (And I did!)
  - Well, after the initial steam went off I had basic entity/player syncing with equipment and meta, basic chunk sending, basic registries w/data driven types that get sent to the client and basic block/item interactions. Literally stopped after seeing that mojang changed the entire protocol from 1.20.1 to 1.20.2, I was very lazy to redo ALL the work I did in two weeks...
  - And here we are, in 2025 writing this README to put an end or indefinite hold to this project without rewriting and porting almost all the code to source generate packets and fix the source generation of BlockState data after some unexpected behaviour, I did all of that just to port the server to 1.21.4 and finally release it to github to have some portfolio or something.
  - (I don't know if this could serve... but meh, someone is going to find this code, which is licensed under MIT license as everything I do, useful)
  - Something which is partially finished and good enough for general use is [Ink.Nbt](https://github.com/GasInfinity/Ink.Nbt), which I finished in 2025 before working on Ink again.

# 🔥 Acknowledgements/Other cool projects
- [Mojang](https://github.com/Mojang): Well, they made minecraft, and this... is a *minecraft* server implementation
- [wiki.vg (Now in minecraft.wiki)](https://minecraft.wiki/w/Minecraft_Wiki:Projects/wiki.vg_merge/Main_Page): The original page will always be remembered, thanks for their contributors and thanks to the minecraft wiki maintainers/contributors for merging it into the main wiki.
- [Minestom](https://minestom.net/): What inspired this basically. 
- [Obsidian](https://docs.obsidianmc.net/index.html): You should check this also. It's written in C# and developed by more people than this.
- [Valence](https://github.com/valence-rs/valence): Maybe I'll contribute to this project instead? (I don't really use rust a lot but looks good)
- [FerrumC](https://github.com/ferrumc-rs/ferrumc): Paper in rust basically, wants to implement vanilla and plugins with ffi

## ‼️ What to know if you want to work on this/fork it/do whatever you want with it (adhering to the LICENSE, ofc)
This was intended/is intending to be a minecraft server implementation and library for development of customized servers (and possibly clients as the main lib is general purpose enough?) with low memory usage/gc pressure in C#.
Important thing, this is was not intending to reimplement Vanilla as modern minecraft is REALLY complex for what is worth the time for just one man. This is intended for being in server lobbies, minigame servers and limbo servers (And even for that you have [Valence](https://github.com/valence-rs/valence))

Do you still want to yank this or use it for I don't know what? Here you have some info:
Atm, all packets are source generated (you've read the little history to know why, didn't you?) with two data files, you'll need to get one which are the packet ids from the official minecraft server data generator, this way if only some id's or minor additions change the protocol, it will be just regenerating the packet id's or adding/removing some packet fields and voila', everything is updated.
Also you need to source generate BlockStates and Registry id's for non network synced registries if you want to use them to test things.

A lot of things here maybe end-up in a separate repo like Ink.Nbt (Ink.Text maybe?)

FINAL DISCLAIMER:
- This project has been only worked on in two times, two weeks in the summer of 2023 and one in 2025. Don't expect commits as this was done just for fun.
- 0 documentation as of this commit (Good Luck!)...

## ✔️ Done
- Seemingly low-memory usage, currently with ALL the block states in memory and ALL the packet info and id mappings with one player inside consumes 10MB with a world height of 16 (the bare minimum). If things are done correctly I see the server running with only 30MB when "fully" done with one world loaded with 10-20 players. Needs profiling for sure.
- Good enough Nbt API that doesnt need intermediary tag handling akin to S.T.J (Ez, but time consuming)
- Implement basic registry support (Ez)
- Basic TextComponent support (Ez)
- Good network handling using only pipelines, buffer writers and spans. Zero streams. (Medium-Hard, not that time consuming)
- Implement compression, encryption and basic authentication. (Medium-Easy)
- Perfect non-allocating state and packet handling that supports NAOT (Medium, lots of time to think of a good design)
- Generate all packets statically but with support for debugging and querying packet info. The only thing needed would be a json that has the description of all packet fields and the dump from the server data generator (Needed for NAOT and for my sanity for updating id's or packets between versions) 
- Pooled chunks/section data and palette. (Ez)
- Good non-allocating low-memory source generated BlockStates with support for querying a property by name and more. (LGTM for now)
- Basic block, item and entity handling. (NEEDS PARTIAL REWRITE)
- Simple chunk batching support

## ❌ TODO
- Document HOW ALL THIS WORKS (Impossible Tier 2), too many files. Making a minecraft server that is aimed at low-memory usage and really wasn't that easy to do...
- Think about what is this project for. Is it a general purpose library and a server or just a server? Thinking about abstractions/apis that support both is both time-consuming and really, really boring. Usually it takes multiple rewrites of one thing for it to be good enough. (Impossible)

- Hey! It seems I could try to prototype an ECS based world... Gotta go fast?
- Hey! It seems blocks and items are MOSTLY data driven internally... Looks like we can take advantage of that :D (Medium)
- Look into inventories and think about how to approach their interface/implementation (Medium-Hard)
- Refactor entity syncing, obviously syncing is only needed on the server, why do we have an IEntityTracker in the general purpose lib?
- Refactor entity physics, is there a better way to do them?
- Refactor ServerPlayerEntity, a lot of things are bad there
- Registry overhaul, currently registry handling internally is very badly done. (Medium, lots of thinking)
- Look into loading automatically generated registry id's into their respective registries or generating Id+Identifier pairs. (Important, Medium)
- Look into DataFixerUpper and their codecs, can't we port it and do something similar? It's MIT (Hard, wtf is a monoid? Codecs may be enough)
- Look into Brigardier, its needed for commands. Port incoming? It's MIT (Ez, but time consuming)
   
---
---
---
I just want to work on a different project and maybe use different programming languages, I've written a lot of C# 💦💦💦💦💦💦
