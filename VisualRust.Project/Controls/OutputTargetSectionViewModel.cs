﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using VisualRust.Cargo;

namespace VisualRust.Project.Controls
{
    public class OutputTargetSectionViewModel: ViewModelBase, IPropertyPageContext
    {
        readonly Manifest manifest;
        // We keep all the targets in a one big collection because
        // CompositeCollection doesn't support grouping
        readonly int libraries = 1;
        int binaries;
        int tests;
        int benchmarks;
        int examples;

        ObservableCollection<IOutputTargetViewModel> targets;
        public ObservableCollection<IOutputTargetViewModel> Targets
        {
            get { return targets; }
            set
            {
                Set(ref targets, value);
            }
        }

        public event EventHandler DirtyChanged;

        private bool isDirty;
        public bool IsDirty
        {
            get { return isDirty; }
            set
            {
                isDirty = value;
                var temp = DirtyChanged;
                if(temp != null)
                    temp(null, null);
            }
        }

        public OutputTargetSectionViewModel(Manifest m)
        {
            this.manifest = m;
            targets = new ObservableCollection<IOutputTargetViewModel>();
            var lookup = manifest.OutputTargets.ToLookup(t => t.Type);
            OutputTarget rawLibraryTarget = lookup[OutputTargetType.Library].FirstOrDefault();
            targets.Add(CreateLibraryTarget(rawLibraryTarget));
            binaries =  LoadTargets(lookup[OutputTargetType.Binary], () => new BinaryAutoOutputTargetViewModel(manifest));
            tests =  LoadTargets(lookup[OutputTargetType.Test], () => new TestAutoOutputTargetViewModel(manifest));
            benchmarks =  LoadTargets(lookup[OutputTargetType.Benchmark], () => new BenchmarkAutoOutputTargetViewModel(manifest));
            examples =  LoadTargets(lookup[OutputTargetType.Example], () => new ExampleAutoOutputTargetViewModel(manifest));
        }

        public void Apply()
        {
            IsDirty = false;
            throw new NotImplementedException();
        }

        IOutputTargetViewModel CreateLibraryTarget(OutputTarget rawLibraryTarget)
        {
            if(rawLibraryTarget == null)
                return new LibraryAutoOutputTargetViewModel(manifest);
            return new OutputTargetViewModel(manifest, rawLibraryTarget);
        }

        int LoadTargets(IEnumerable<OutputTarget> rawTargets, Func<IOutputTargetViewModel> ctor)
        {
            List<OutputTargetViewModel> vms = rawTargets.Select(t => new OutputTargetViewModel(manifest, t)).ToList();
            if(vms.Count == 0)
            {
                Targets.Add(ctor());
                return 1;
            }
            else
            {
                foreach(OutputTargetViewModel vm in vms)
                    Targets.Add(vm);
                return vms.Count;
            }
        }
    }
}
