
# Gebouweenheden status incorrect tegenover gebouw

https://vlaamseoverheid.atlassian.net/browse/GAWR-6413

## Context
We hebben twee problemen gedetecteerd die niet werden opgelost bij migratie vanuit CRAB.

1. Er waren gebouweenheden met status `gerealiseerd`, maar het gebouw had nog (maar) status `gepland` of `inAanbouw`.

```
select bu.BuildingUnitPersistentLocalId As Id from [building-registry].buildingregistrylegacy.BuildingDetailsV2 b
inner join [building-registry].BuildingRegistryLegacy.BuildingUnitDetailsV2 bu on b.PersistentLocalId = bu.BuildingPersistentLocalId
where b.Status in ('Planned', 'UnderConstruction') and bu.Status in ('Realized') and bu.IsRemoved = 0 and bu.[Function] = 'Unknown'
```

2. Er waren gebouweenheden met status `gepland` of `gerealiseerd` in een `nietGerealiseerd` gebouw.

```
select bu.BuildingUnitPersistentLocalId As Id from [building-registry].buildingregistrylegacy.BuildingDetailsV2 b
inner join [building-registry].BuildingRegistryLegacy.BuildingUnitDetailsV2 bu on b.PersistentLocalId = bu.BuildingPersistentLocalId
where b.Status in ('NotRealized') and bu.Status in ('Realized', 'Planned') and bu.IsRemoved = 0 and bu.[Function] = 'Unknown'
```

## Oplossing probleem 1

Gebouweenheden met status `gerealiseerd` corrigeren naar `gepland`.

* input_prd_gerealiseerd_naar_gepland.csv
* output_prd_gerealiseerd_naar_gepland.csv

## Oplossing probleem 2

Gebouweenheden met status `gerealiseerd` corrigeren naar `gepland`.

Daarna gebouweenheden met status `gepland` niet-realiseren.

* input_prd_gerealiseerd_naar_gepland_naar_nietGerealiseerd.csv
* output_prd_gerealiseerd_naar_gepland_naar_nietGerealiseerd_stap1.csv
* output_prd_gerealiseerd_naar_gepland_naar_nietGerealiseerd_stap2.csv