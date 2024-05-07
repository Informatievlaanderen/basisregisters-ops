
# Gehistoreerde or niet gerealiseerde of verwijderde gebouweenheden hebben adressen

https://vlaamseoverheid.atlassian.net/browse/GAWR-6419

## Context
Vanuit migratie CRAB is het probleem meegekomen.

Er waren gebouweenheden met status `gehistoreerd` of `NietGerealiseerd` of zijn verwijderd, maar hebben nog een adres koppeling.

```
SELECT
        bua.building_unit_persistent_local_id as "Id"                
        , 'https://api.basisregisters.vlaanderen.be/v2/gebouweenheden/' || bua.building_unit_persistent_local_id || '/acties/adresontkoppelen' as "Url"
				, '{ "adresId": "https://data.vlaanderen.be/id/adres/' || a.persistent_local_id || '"}' as "Body"
FROM integration_building.building_unit_addresses as bua
JOIN integration_building.building_unit_latest_items as bu
ON bua.building_unit_persistent_local_id = bu.building_unit_persistent_local_id
  and (bu.status in ('NotRealized','Retired') or bu.is_removed = true)
JOIN integration_address.address_latest_items as a
  on bua.address_persistent_local_id = a.persistent_local_id
```

## Oplossing

Ontkoppelen van de adressen

* input_prd_gerealiseerd_naar_gepland.csv
* output_prd_gerealiseerd_naar_gepland.csv