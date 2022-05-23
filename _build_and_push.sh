#!/bin/bash
IID_FILE=image.iid
TAG=$(date +%Y%m%d%H%M%S)

buildah bud -f Containerfile -t maw-size-photos --iidfile "${IID_FILE}"

IID=$(cat "${IID_FILE}")

buildah push --creds "${DH_USER}:${DH_PASS}" "${IID}" docker://aerisg222/maw-size-photos:"${TAG}"
buildah push --creds "${DH_USER}:${DH_PASS}" "${IID}" docker://aerisg222/maw-size-photos:latest
