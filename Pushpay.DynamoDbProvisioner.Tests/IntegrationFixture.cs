//using System;
//using System.Threading.Tasks;
//using Amazon.DynamoDBv2.DataModel;
//using Autofac;
//using Pushpay.DynamoDbProvisioner.Provisioner;
//using Pushpay.DynamoDbProvisioner.Provisioner.Utils;
//using Xunit;
//using IContainer = Autofac.IContainer;
//
//namespace Pushpay.DynamoDbProvisioner.Tests
//{
//    public class IntegrationFixture : IAsyncLifetime
//    {
//        const int DynamoPort = 4569;
//
//        public IntegrationFixture()
//        {
//            var containerBuilder = new ContainerBuilder();
//
//            var config = new AwsInitializationConfiguration
//            {
//                DynamoModelAssemblies = new[] {typeof(DynamoDbProvisioner.Provisioner.DynamoDbProvisioner).Assembly},
//                DynamoTableNamingStrategy = TableNamingStrategy.Random,
//                UseLocalStack = c => true,
//            };
//            DynamoConfigurationUtils.RegisterDynamoDb(containerBuilder, config.DynamoTablePrefix, config.UseLocalStack,
//                c => BuildServiceUrl(c, config.LocalStackHost, DynamoPort), config.DynamoTableNamingStrategy, config.DynamoModelAssemblies);
//            
//            Container = containerBuilder.Build();
//
//        }
//
//        public IContainer Container { get; }
//
//        public IDynamoDBContext DynamoDbContext => Container.Resolve<IDynamoDBContext>();
//
//        public static string BuildServiceUrl(IComponentContext c, Func<IComponentContext, string> localstackHost, int port)
//        {
//            string host = localstackHost == null ? "localhost" : localstackHost(c) ?? "localhost";
//            return "http://" + host + ":" + port;
//        }
//
//        public Task InitializeAsync()
//        {
//            return Container.Resolve<DynamoDbProvisioner.Provisioner.DynamoDbProvisioner>().Setup(performFirstTimeTestTableCleanup: true);
//        }
//
//        public Task DisposeAsync()
//        {
//            return Container.Resolve<DynamoDbProvisioner.Provisioner.DynamoDbProvisioner>().Cleanup();
//        }
//    }
//}