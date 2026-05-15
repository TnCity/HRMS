using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRMS.DAL.Repositories;
using HRMS.Entities;

namespace HRMS.BLL.Services
{
    public class JobService
    {
        private readonly IJobRepository _repo;

        public JobService(IJobRepository repo)
        {
            _repo = repo;
        }

        public List<Job> GetJobs()
        {
            return _repo.GetAll();
        }

        public void AddJob(Job job)
        {
            _repo.Add(job);
            _repo.Save();
        }

        public Job GetJobById(int id)
        {
            return _repo.GetById(id);
        }

        public void UpdateJob(Job job)
        {
            _repo.Update(job);
            _repo.Save();
        }

        public void DeleteJob(int id)
        {
            _repo.Delete(id);
            _repo.Save();
        }
    }
}
