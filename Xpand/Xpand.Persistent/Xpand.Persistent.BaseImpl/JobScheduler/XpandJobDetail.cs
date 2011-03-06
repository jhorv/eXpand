﻿using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Quartz;
using Xpand.ExpressApp.Attributes;
using Xpand.Persistent.Base.General;
using Xpand.Persistent.Base.General.CustomAttributes;
using Xpand.Persistent.Base.JobScheduler;
using Xpand.Persistent.Base.JobScheduler.Triggers;
using Xpand.Persistent.BaseImpl.JobScheduler.Triggers;
using Xpand.Xpo;

namespace Xpand.Persistent.BaseImpl.JobScheduler {

    [DefaultClassOptions]
    [System.ComponentModel.DisplayName("JobDetail")]
    [Appearance("Disable_Name_For_XpandJobDetail_ExistingObjects", AppearanceItemType.ViewItem, "IsNewObject=false", TargetItems = "Name", Enabled = false)]
    [Appearance("Disable_Job_For_XpandJobDetail_ExistingObjects", AppearanceItemType.ViewItem, "IsNewObject=false", TargetItems = "Job", Enabled = false)]
    [Appearance("Disable_Group_For_XpandJobDetail_ExistingObjects", AppearanceItemType.ViewItem, "IsNewObject=false", TargetItems = "Group", Enabled = false)]
    public class XpandJobDetail : XpandCustomObject, IJobDetail, IFastManyToMany {
        public XpandJobDetail(Session session)
            : base(session) {
        }
        private string _name;
        [RuleRequiredField]
        [RuleUniqueValue(null, DefaultContexts.Save)]
        public string Name {
            get {
                return _name;
            }
            set {
                SetPropertyValue("Name", ref _name, value);
            }
        }
        [Tooltip("Whether or not the IJob implements the interface IStatefulJob.")]
        public virtual bool Stateful {
            get {
                return _job != null && (XafTypesInfo.Instance.FindTypeInfo(typeof(IStatefulJob)).Type.IsAssignableFrom(_job.JobType));
            }
        }


        private string _description;
        [Size(SizeAttribute.Unlimited)]
        public string Description {
            get {
                return _description;
            }
            set {
                SetPropertyValue("Description", ref _description, value);
            }
        }
        private XpandJob _job;
        [ProvidedAssociation("XpandJob-XpandJobDetails")]
        [RuleRequiredField]
        public XpandJob Job {
            get {
                return _job;
            }
            set {
                SetPropertyValue("Job", ref _job, value);
            }
        }
        IXpandJob IJobDetail.Job {
            get { return _job; }
            set { _job = value as XpandJob; }
        }

        private XpandJobDataMap _jobDataMap;
        [Browsable(false)]
        public XpandJobDataMap JobDataMap {
            get {
                return _jobDataMap;
            }
            set {
                SetPropertyValue("JobDataMap", ref _jobDataMap, value);
            }
        }
        private bool _requestsRecovery;
        [Tooltip("Whether or not the the IScheduler should re-Execute the IJob if a 'recovery' or 'fail-over' situation is encountered.")]
        public bool RequestsRecovery {
            get {
                return _requestsRecovery;
            }
            set {
                SetPropertyValue("RequestsRecovery", ref _requestsRecovery, value);
            }
        }
        

        [Association("JobDetailTriggerLink-Triggers"), Aggregated]
        protected IList<JobDetailTriggerLink> TriggerLinks {
            get {
                return GetList<JobDetailTriggerLink>("TriggerLinks");
            }
        }
        [ManyToManyAlias("TriggerLinks", "JobTrigger")]
        public IList<XpandJobTrigger> JobTriggers {
            get { return GetList<XpandJobTrigger>("JobTriggers"); }
        }
        IList<IJobTrigger> IJobDetail.JobTriggers {
            get {
                return new ListConverter<IJobTrigger, XpandJobTrigger>(JobTriggers);
            }
        }
        [Association("JobDetailJobListenerTriggerLink-JobListeners"), Aggregated]
        protected IList<JobDetailJobListenerTriggerLink> JobListenerTriggerLinks {
            get {
                return GetList<JobDetailJobListenerTriggerLink>("JobListenerTriggerLinks");
            }
        }
        [ManyToManyAlias("JobListenerTriggerLinks", "JobListenerTrigger")]
        public IList<JobListenerTrigger> JobListenerTriggers {
            get { return GetList<JobListenerTrigger>("JobListenerTriggers"); }
        }
        private JobSchedulerGroup _group;
        [ProvidedAssociation("JobSchedulerGroup-XpandJobDetails")]
        public JobSchedulerGroup Group {
            get {
                return _group;
            }
            set {
                SetPropertyValue("Group", ref _group, value);
            }
        }
        IJobSchedulerGroup IJobDetail.Group {
            get { return Group; }
            set { Group = value as JobSchedulerGroup; }
        }

        IList<IJobListenerTrigger> IJobDetail.JobListenerTriggers {
            get {
                return new ListConverter<IJobListenerTrigger, JobListenerTrigger>(JobListenerTriggers);
            }
        }
    }
}
