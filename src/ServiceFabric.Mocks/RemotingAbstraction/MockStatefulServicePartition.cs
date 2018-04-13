using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;

namespace ServiceFabric.Mocks.RemotingAbstraction
{
    /// <summary>
    /// Mock implementation for <see cref="IStatefulServicePartition"/>.
    /// </summary>
    public class MockStatefulServicePartition : IStatefulServicePartition
    {

        public MockStatefulServicePartition()
        {
            PartitionInfo = MockQueryPartitionFactory.CreateIntPartitonInfo();
        }

        public void ReportLoad(IEnumerable<LoadMetric> metrics)
        {
        }

        public void ReportFault(FaultType faultType)
        {
        }

        public void ReportMoveCost(MoveCost moveCost)
        {
        }

        public void ReportPartitionHealth(HealthInformation healthInfo)
        {
        }

        public void ReportPartitionHealth(HealthInformation healthInfo, HealthReportSendOptions sendOptions)
        {
        }

        public ServicePartitionInformation PartitionInfo { get; set; }

        public FabricReplicator CreateReplicator(IStateProvider stateProvider, ReplicatorSettings replicatorSettings)
        {
            return null;
        }

        public void ReportReplicaHealth(HealthInformation healthInfo)
        {
        }

        public void ReportReplicaHealth(HealthInformation healthInfo, HealthReportSendOptions sendOptions)
        {
        }

        public PartitionAccessStatus ReadStatus { get; set; } = PartitionAccessStatus.Granted;

        public PartitionAccessStatus WriteStatus { get; set; } = PartitionAccessStatus.Granted;
    }
}