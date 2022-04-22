﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Tools.Monitor.CollectionRules.Actions;
using Microsoft.Diagnostics.Tools.Monitor.CollectionRules.Options;
using Microsoft.Diagnostics.Tools.Monitor.CollectionRules.Triggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Microsoft.Diagnostics.Tools.Monitor.CollectionRules.Configuration
{
    internal sealed class CustomShortcutsConfigureOptions :
        IPostConfigureOptions<CollectionRuleOptions>
    {
        private readonly ICollectionRuleTriggerOperations _triggerOperations;
        private readonly ICollectionRuleActionOperations _actionOperations;
        private readonly IOptionsMonitor<CustomShortcutOptions> _shortcutOptions;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CustomShortcutsConfigureOptions> _logger;

        public CustomShortcutsConfigureOptions(
            ICollectionRuleTriggerOperations triggerOperations,
            ICollectionRuleActionOperations actionOperations,
            IOptionsMonitor<CustomShortcutOptions> shortcutOptions,
            IConfiguration configuration,
            ILogger<CustomShortcutsConfigureOptions> logger)
        {
            _triggerOperations = triggerOperations;
            _actionOperations = actionOperations;
            _shortcutOptions = shortcutOptions;
            _configuration = configuration;
            _logger = logger;
        }

        public void PostConfigure(string name, CollectionRuleOptions options)
        {
            // Action Shortcuts
            foreach (var key in _shortcutOptions.CurrentValue.Actions.Keys)
            {
                IConfigurationSection section = _configuration.GetSection($"{nameof(RootOptions.CustomShortcuts)}:{nameof(CustomShortcutOptions.Actions)}:{key}");

                if (section.Exists())
                {
                    section.Bind(_shortcutOptions);

                    BindCustomActions(section, _shortcutOptions.CurrentValue, key);
                }
            }

            // Trigger Shortcuts
            foreach (var key in _shortcutOptions.CurrentValue.Triggers.Keys)
            {
                IConfigurationSection section = _configuration.GetSection($"{nameof(RootOptions.CustomShortcuts)}:{nameof(CustomShortcutOptions.Triggers)}:{key}");

                if (section.Exists())
                {
                    section.Bind(_shortcutOptions);

                    BindCustomTriggers(section, _shortcutOptions.CurrentValue, key);
                }
            }

            bool actionInsertion = InsertCustomActionsIntoActionList(options, name);
            bool triggerInsertion = InsertCustomTriggerIntoTrigger(options, name);
            bool filterInsertion = InsertCustomFiltersIntoFilterList(options, name);
            bool limitInsertion = InsertCustomLimitIntoLimit(options, name);

            // If any insertions fail, clear out the options so that the rule will fail Validation
            if (!(actionInsertion && triggerInsertion && filterInsertion && limitInsertion))
            {
                options.Trigger = null;
                options.Actions.Clear();
                options.Filters.Clear();
                options.Limits = null;
            }
        }

        private bool InsertCustomActionsIntoActionList(CollectionRuleOptions ruleOptions, string name)
        {
            bool boundAllActions = false;
            int index = 0;

            while (!boundAllActions)
            {
                IConfigurationSection section = _configuration.GetSection($"{nameof(RootOptions.CollectionRules)}:{name}:{nameof(CollectionRuleOptions.Actions)}:{index}");
                if (section.Exists() && !string.IsNullOrEmpty(section.Value))
                {
                    if (!InsertCustomShortcut(_shortcutOptions.CurrentValue.Actions, section.Value, ruleOptions.Actions, index))
                    {
                        return false;
                    }
                }
                else if (!section.Exists())
                {
                    boundAllActions = true;
                }

                ++index;
            }

            return true;
        }

        private bool InsertCustomTriggerIntoTrigger(CollectionRuleOptions ruleOptions, string name)
        {
            IConfigurationSection section = _configuration.GetSection($"{nameof(RootOptions.CollectionRules)}:{name}:{nameof(CollectionRuleOptions.Trigger)}");
            if (section.Exists() && !string.IsNullOrEmpty(section.Value))
            {
                if (!ValidateCustomShortcutExists(_shortcutOptions.CurrentValue.Triggers, section.Value))
                {
                    return false;
                }

                CollectionRuleTriggerOptions options = _shortcutOptions.CurrentValue.Triggers[section.Value]; // Need to make sure this is safe for invalid names
                ruleOptions.Trigger = options;
            }

            return true;
        }

        private bool InsertCustomFiltersIntoFilterList(CollectionRuleOptions ruleOptions, string name)
        {
            bool boundAllFilters = false;
            int index = 0;

            while (!boundAllFilters)
            {
                IConfigurationSection section = _configuration.GetSection($"{nameof(RootOptions.CollectionRules)}:{name}:{nameof(CollectionRuleOptions.Filters)}:{index}");
                if (section.Exists() && !string.IsNullOrEmpty(section.Value))
                {
                    if (!InsertCustomShortcut(_shortcutOptions.CurrentValue.Filters, section.Value, ruleOptions.Filters, index))
                    {
                        return false;
                    }
                }
                else if (!section.Exists())
                {
                    boundAllFilters = true;
                }

                ++index;
            }

            return true;
        }

        private bool InsertCustomLimitIntoLimit(CollectionRuleOptions ruleOptions, string name)
        {
            IConfigurationSection section = _configuration.GetSection($"{nameof(RootOptions.CollectionRules)}:{name}:{nameof(CollectionRuleOptions.Limits)}");
            if (section.Exists() && !string.IsNullOrEmpty(section.Value))
            {
                if (!ValidateCustomShortcutExists(_shortcutOptions.CurrentValue.Limits, section.Value))
                {
                    return false;
                }

                CollectionRuleLimitsOptions options = _shortcutOptions.CurrentValue.Limits[section.Value]; // Need to make sure this is safe for invalid names
                ruleOptions.Limits = options;
            }

            return true;
        }

        private bool InsertCustomShortcut<T>(IDictionary<string, T> shortcutsOptions, string shortcutKey, List<T> options, int index) where T : class
        {
            if (!ValidateCustomShortcutExists(shortcutsOptions, shortcutKey))
            {
                return false;
            }

            options.Insert(index, shortcutsOptions[shortcutKey]);
            return true;
        }

        private bool ValidateCustomShortcutExists<T>(IDictionary<string, T> shortcutsOptions, string shortcutKey)
        {
            if (shortcutsOptions.ContainsKey(shortcutKey))
            {
                return true;
            }

            _logger.LogError(Strings.ErrorMessage_CustomShortcutNotFound, shortcutKey);
            return false;
        }

        private void BindCustomActions(IConfigurationSection ruleSection, CustomShortcutOptions shortcutOptions, string shortcutName)
        {
            CollectionRuleActionOptions actionOptions = shortcutOptions.Actions[shortcutName];

            if (null != actionOptions &&
                _actionOperations.TryCreateOptions(actionOptions.Type, out object actionSettings))
            {
                IConfigurationSection settingsSection = ruleSection.GetSection(ConfigurationPath.Combine(
                    nameof(CollectionRuleActionOptions.Settings)));

                settingsSection.Bind(actionSettings);

                actionOptions.Settings = actionSettings;
            }
        }

        private void BindCustomTriggers(IConfigurationSection ruleSection, CustomShortcutOptions shortcutOptions, string shortcutName)
        {
            CollectionRuleTriggerOptions triggerOptions = shortcutOptions.Triggers[shortcutName];

            if (null != triggerOptions &&
                _triggerOperations.TryCreateOptions(triggerOptions.Type, out object triggerSettings))
            {
                IConfigurationSection settingsSection = ruleSection.GetSection(ConfigurationPath.Combine(
                    nameof(CollectionRuleTriggerOptions.Settings)));

                settingsSection.Bind(triggerSettings);

                triggerOptions.Settings = triggerSettings;
            }
        }
    }
}
