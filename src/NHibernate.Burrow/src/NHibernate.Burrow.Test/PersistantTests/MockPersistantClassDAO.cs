using NHibernate.Burrow.Util.DAOBases;

namespace NHibernate.Burrow.Test.PersistantTests {
    public class MockPersistantClassDAO : GenericDAOBase<MockPersistantClass> {
        private static readonly MockPersistantClassDAO instance = new MockPersistantClassDAO();

        public static MockPersistantClassDAO Instance {
            get { return instance; }
        }

        // Uncomment the following method if your entity has a unique Name property 
        // public MockPersistantClass FindByName(string name) {
        //     return (MockPersistantClass) GetCriteria().Add(Expression.Eq("Name",name))
        //                             .UniqueResult();
        // }
    }
}