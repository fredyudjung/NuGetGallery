﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Services.Work.Models;
using PowerArgs;

namespace NuCmd.Commands.Work
{
    [Description("Gets invocations in various places in the queue")]
    public class GetCommand : WorkServiceCommandBase
    {
        [ArgRequired()]
        [ArgShortcut("c")]
        [ArgPosition(0)]
        [ArgDescription("The ID of the invocation to get more information on. Or a criteria value, one of 'all', 'active', 'completed', 'executing', 'pending', 'hidden', 'suspended'")]
        public string CriteriaOrId { get; set; }

        protected override Task OnExecute()
        {
            // Try to parse the criteria
            InvocationListCriteria criteria;
            if (Enum.TryParse<InvocationListCriteria>(CriteriaOrId, ignoreCase: true, result: out criteria))
            {
                return GetByCriteria(criteria);
            }
            else
            {
                return GetById(CriteriaOrId);
            }
        }

        private async Task GetById(string id)
        {
            var client = await OpenClient();
            if (client == null) { return; }

            var response = await client.Invocations.Get(id);

            if (await ReportHttpStatus(response))
            {
                await Console.WriteObject(await response.ReadContent());
            }
        }

        private async Task GetByCriteria(InvocationListCriteria criteria)
        {
            var client = await OpenClient();
            if (client == null) { return; }

            var response = await client.Invocations.Get(criteria);

            if (await ReportHttpStatus(response))
            {
                var invocations = await response.ReadContent();
                await Console.WriteTable(invocations, i => new {
                    i.Job,
                    i.Status,
                    i.Result,
                    i.Id,
                    i.QueuedAt,
                    i.UpdatedAt,
                    i.CompletedAt
                });
            }
        }
    }
}