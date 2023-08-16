#!/bin/sh

CONTAINERID=$(curl -s http://169.254.170.2/v2/metadata | jq -r ".Containers[] | select(.Labels[\"com.amazonaws.ecs.container-name\"] | startswith(\"ops\") and endswith(\"-web\")) | .DockerId")

sed -i "s/REPLACE_CONTAINERID/$CONTAINERID/g" appsettings.json

./Ops.Web
