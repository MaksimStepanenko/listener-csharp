using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestIt.WebApi.Models;

namespace TestIT.Listener
{
    class Program
    {
        private const int DeleyInMilliseconds = 1000;

        public static readonly string Domain = "http://192.168.88.51";
        public static readonly string SecretKey = "QzM3ejdzUUZ2NTdNc0RTRA==";
        public static readonly string UserName = "auto";
        public static readonly string PathToTests = @"D:\autotests\testit";

        static async Task Main(string[] args)
        {
            // ������ ��� �������� ��������
            PublicApiClient client = new PublicApiClient(Domain, SecretKey, UserName);
            // ��������� ������ ��� ������� ����������
            TestRunner runner = new TestRunner();
            // ��������� ������ ��� ��������� ����������� ����������
            AutoTestResultsProvider resultsProvider = new AutoTestResultsProvider();
            // ����������� ���� ��� ��������� � ��������� ������� �� ������ ����������
            while (true)
            {
                // ��������� ���� �������� ��������
                IEnumerable<PublicTestRunModel> testRuns = await client.GetAllActiveTestRuns();
                // ��������� ������� ��������������� ��������� ������� 
                PublicTestRunModel run = testRuns.FirstOrDefault(testRun => testRun.Status == TestRunStates.NotStarted);
                // ���� �������� ������ ��� ������, ������������ ���
                if (run != null)
                {
                    // �������� �������� ������, ��������� �������� ������ � ������  In Progress
                    await client.StartTestRun(run.TestRunId.ToString());
                    // �������� �������� ������ (�� ���������������)
                    List<string> testsName = run.AutoTests.Select(at => at.AutotestExternalId).ToList();
                    // �������� ��������� ���������
                    runner.RunSelectedTests(PathToTests, testsName);
                    // �������� ���������� ����������
                    IEnumerable<TestResult> testResults = resultsProvider.GetLastResults(PathToTests);
                    // ����������� ���������� ����������
                    await client.SetAutoTestResult(run, testResults);
                }
                // ��������
                Thread.Sleep(DeleyInMilliseconds);
            }
        }
    }
}
