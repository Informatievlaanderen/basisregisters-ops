# Adressen goed te keuren in antwerpen

https://vlaamseoverheid.atlassian.net/browse/GAWR-6690

## Context
Tijdens fusie zijn er adressen in bestaande straatnamen geplaatst.
Omdat die straatnamen al actief zijn is er geen event die via fusie komt om het actief te maken.

Daarom moeten we de adressen goedkeuren, maar enkel als ze al in de oude gemeente in status `inGebruik` zijn stonden.

## Oplossing

Adressen op 1/1/2025 `inGebruik` plaatsen als ze in de oude gemeente in status `inGebruik` stonden.
In de csv staan ook adressen die al goedgekeurd zijn via fusie events, deze zullen dus idempotent zijn en geen event gooien.

* adressen-antwerpen.csv
* output_prd_gerealiseerd_naar_gepland.csv