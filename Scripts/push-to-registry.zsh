#!/bin/zsh

# Default values
LOCAL_IMAGE_NAME="infrastructure-api"
REGISTRY_URL="cr.selcloud.ru"
REGISTRY_REPO="selfix-beta-registry"
IMAGE_NAME="selfix-beta-infrastructure-api-app"
TAG="latest"
BUILD_CONTEXT="$(dirname "$0")/../Selfix"  # Default to Selfix directory
PLATFORM="linux/amd64"  # Default platform

# Parse command line arguments
while [[ $# -gt 0 ]]; do
  case $1 in
    --local-image-name)
      LOCAL_IMAGE_NAME="$2"
      shift 2
      ;;
    --registry-url)
      REGISTRY_URL="$2"
      shift 2
      ;;
    --registry-repo)
      REGISTRY_REPO="$2"
      shift 2
      ;;
    --image-name)
      IMAGE_NAME="$2"
      shift 2
      ;;
    --tag)
      TAG="$2"
      shift 2
      ;;
    --build-context)
      BUILD_CONTEXT="$2"
      shift 2
      ;;
    --platform)
      PLATFORM="$2"
      shift 2
      ;;
    *)
      echo "Unknown option: $1"
      exit 1
      ;;
  esac
done

# Construct the full registry path
REGISTRY_PATH="$REGISTRY_URL/$REGISTRY_REPO/$IMAGE_NAME:$TAG"

# Display execution plan
echo -e "\033[32mDeploying container with the following parameters:\033[0m"
echo -e "\033[36m  Local Image Name: $LOCAL_IMAGE_NAME\033[0m"
echo -e "\033[36m  Registry URL: $REGISTRY_URL\033[0m"
echo -e "\033[36m  Registry Repository: $REGISTRY_REPO\033[0m"
echo -e "\033[36m  Image Name: $IMAGE_NAME\033[0m"
echo -e "\033[36m  Tag: $TAG\033[0m"
echo -e "\033[36m  Full Registry Path: $REGISTRY_PATH\033[0m"
echo -e "\033[36m  Build Context: $BUILD_CONTEXT\033[0m"
echo -e "\033[36m  Platform: $PLATFORM\033[0m"

# Verify Dockerfile exists in build context
if [[ ! -f "$BUILD_CONTEXT/Dockerfile" ]]; then
    echo -e "\033[31mError: Dockerfile not found in build context: $BUILD_CONTEXT\033[0m"
    exit 1
fi

echo -e "\n\033[33m1. Building Docker image...\033[0m"
docker build --platform "$PLATFORM" -t "$REGISTRY_PATH" "$BUILD_CONTEXT"
if [ $? -ne 0 ]; then
    echo -e "\033[31mError: Docker build failed!\033[0m"
    exit 1
fi

echo -e "\n\033[33m2. Pushing Docker image to registry...\033[0m"
docker push "$REGISTRY_PATH"
if [ $? -ne 0 ]; then
    echo -e "\033[31mError: Docker push failed!\033[0m"
    exit 1
fi

echo -e "\n\033[32mDeployment completed successfully!\033[0m" 