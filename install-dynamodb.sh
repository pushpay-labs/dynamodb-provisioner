curl https://download.java.net/java/GA/jdk11/9/GPL/openjdk-11.0.2_linux-x64_bin.tar.gz -o /dynamodb-provisioner/openjdk-11.0.2_linux-x64_bin.tar.gz
tar -xzf /dynamodb-provisioner/openjdk-11.0.2_linux-x64_bin.tar.gz

export AWS_DEFAULT_REGION=us-west-2
export AWS_SECRET_ACCESS_KEY="fakeSecretAccessKey"
export AWS_ACCESS_KEY_ID="fakeMyKeyId"
java --version

curl "https://s3-us-west-2.amazonaws.com/dynamodb-local/dynamodb_local_latest.zip" -o "/dynamodb-provisioner/dynamodb_local_latest.zip"
unzip /dynamodb-provisioner/dynamodb_local_latest.zip

java -Djava.library.path=DynamoDBLocal_lib -jar /dynamodb-provisioner/DynamoDBLocal.jar -sharedDb &