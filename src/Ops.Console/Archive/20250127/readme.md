# Ontkoppelen verwijderde adressen van perceel

https://vlaamseoverheid.atlassian.net/browse/GAWR-6746

## Context
Er waren adressen die verwijderd werden, maar nog aan een perceel hangen.
Deze adressen moeten ontkoppeld worden van het perceel.
Dit kwam omdat de importer van de percelen geen rekening hield met `isRemoved` van adressen.

```sql
SELECT d.CaPaKey, a.AddressPersistentLocalId
  FROM [parcel-registry].[ParcelRegistryLegacy].[ParcelDetailAddresses] p
  inner join [parcel-registry].ParcelRegistryLegacy.ParcelDetails d on p.ParcelId = d.ParcelId
  inner join [parcel-registry-events].ParcelRegistryConsumerAddress.Addresses a on a.AddressPersistentLocalId = p.AddressPersistentLocalId
  where a.IsRemoved = 1
```

## Oplossing

Ontkoppel de adressen van de percelen.

* input-ontkoppel-adres-perceel.csv
* output-ontkoppel-adres-perceel.csv