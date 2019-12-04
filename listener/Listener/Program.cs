using System;
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

        public static readonly string Domain = "";
        public static readonly string SecretKey = "";
        public static readonly string PathToTests = @"D:\e2e_tests\";
        public static readonly List<string> Errors = new List<string>();

        static async Task Main()
        {
            // ������ ��� �������� ��������
            PublicApiClient client = new PublicApiClient(Domain, SecretKey);
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
                    try
                    {
                        // �������� �������� ������, ��������� �������� ������ � ������  In Progress
                        await client.StartTestRun(run.TestRunId.ToString());
                        // �������� �������� ������ (�� ���������������)
                        List<string> testsName = run.AutoTests.Select(at => at.ExternalId).ToList();
                        // �������� ��������� ���������
                        runner.RunSelectedTests(PathToTests, testsName);
                        // �������� ���������� ����������
                        IEnumerable<TestResult> testResults = resultsProvider.GetLastResults(PathToTests);
                        // ����������� ���������� ����������
                        await client.SetAutoTestResult(run, testResults);
                        //��������� ������ ����� ������������ ���� �����������
                        await client.CompleteTestRun(run.TestRunId.ToString());
                    }
                    catch (Exception ex)
                    {
                        Errors.Add(ex.Message);
                        //��������� ������ � ������ ������
                        await client.CompleteTestRun(run.TestRunId.ToString());
                    }
                }
                // ��������
                Thread.Sleep(DeleyInMilliseconds);
            }
        }
    }
}
