﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WzComparerR2.Config
{
    public static class ConfigManager
    {
        public static string ConfigFileName
        {
            get { return Path.Combine(Application.StartupPath, "Setting.config"); }
        }

        public static Configuration ConfigFile
        {
            get { return _configFile = (_configFile ?? Open()); }
        }

        private static Configuration _configFile;

        private static Configuration Open()
        {
            var configFile = ConfigurationManager.OpenMappedMachineConfiguration(
                new ConfigurationFileMap()
                {
                    MachineConfigFilename = ConfigFileName
                });

            return configFile;
        }

        public static void Reload()
        {
            _configFile = Open();
        }

        public static void Save()
        {
            _configFile?.Save(ConfigurationSaveMode.Full);
        }

        public static bool RegisterSection<T>() where T : ConfigSectionBase<T>, new()
        {
            return RegisterSection(typeof(T));
        }

        public static bool RegisterSection(Type type)
        {
            if (type == null || !type.IsSubclassOf(typeof(ConfigSectionBase<>).MakeGenericType(type)))
            {
                throw new ArgumentException($"类型{type}没有继承于{typeof(ConfigSectionBase<>)}。");
            }

            string secName = GetSectionName(type);
          
            if (ConfigFile.GetSection(secName) == null)
            {
                ConfigFile.Sections.Add(secName, Activator.CreateInstance(type) as ConfigurationSection);
                var section = ConfigFile.GetSection(secName);
                return true;
            }
            return false;
        }

        public static void RegisterAllSection()
        {
            var asm = Assembly.GetCallingAssembly();
            RegisterAllSection(asm);
        }
        public static void RegisterAllSection(Assembly assembly)
        {
            var secTypes = assembly.GetExportedTypes().Where(type => {
                try
                {
                    var baseType = type.BaseType;
                    return baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(ConfigSectionBase<>);
                }
                catch
                {
                    return false;
                }
            });
            bool needSave = false;
            foreach (var type in secTypes)
            {
                needSave |= RegisterSection(type);
            }

            if (needSave)
            {
                Save();
            }
        }

        public static string GetSectionName(Type type)
        {
            var attrList = type.GetCustomAttributes(typeof(SectionNameAttribute), false).OfType<SectionNameAttribute>();
            return attrList.Select(attr => attr.Name).FirstOrDefault(secName => !string.IsNullOrEmpty(secName)) ?? type.Name;
        }
    }
}
