docker tag $CONTAINER_RELEASE_IMAGE:latest $CONTAINER_RELEASE_IMAGE:$1
docker push $CONTAINER_RELEASE_IMAGE:$1
docker push $CONTAINER_RELEASE_IMAGE:latest