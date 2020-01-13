namespace MosPolyHelper.Adapters
{
    using Android.Support.V7.Widget;
    using Android.Views;
    using Android.Widget;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    class AdvancedSearchAdapter : RecyclerView.Adapter
    {
        IList<string> dataSet;
        readonly IAdvancedSearchFilter filter;

        public AdvancedSearchAdapter(IAdvancedSearchFilter filter)
        {
            this.filter = filter;
            this.dataSet = filter.GetFiltered(null);
        }

        public override int ItemCount => this.dataSet?.Count ?? 0;

        public void UpdateTemplate(string template)
        {
            this.dataSet = filter.GetFiltered(template);
            NotifyDataSetChanged();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_advanced_search, parent, false);
            var vh = new ViewHolder(view, CheckBoxChanged);
            return vh;
        }

        void CheckBoxChanged(int position, bool isChecked)
        {
            filter.SetChecked(this.dataSet[position], isChecked);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            if (!(vh is ViewHolder viewHolder))
            {
                return;
            }
            viewHolder.CheckBox.Text = this.dataSet[position];
            viewHolder.CheckBox.Checked = this.filter.GetChecked(this.dataSet[position]);
        }

        public class ViewHolder : RecyclerView.ViewHolder
        {
            public CheckBox CheckBox { get; }

            public ViewHolder(View v, Action<int, bool> checkedChange) : base(v)
            {
                this.CheckBox = v.FindViewById<CheckBox>(Resource.Id.checkBox);
                this.CheckBox.CheckedChange += (obj, arg) => checkedChange(this.LayoutPosition, arg.IsChecked);
            }
        }

        public class AdvancedFilter : IAdvancedSearchFilter
        {
            readonly IList<string> originDataSet;
            readonly Dictionary<string, (string Normalized, bool IsChecked)> dataSetDictionary;
            readonly List<string> checkedList;

            public AdvancedFilter(IList<string> originDataSet, List<string> checkedList)
            {
                this.originDataSet = originDataSet;
                this.dataSetDictionary = new Dictionary<string, (string, bool)>(originDataSet.Count);
                foreach (string str in originDataSet)
                {
                    this.dataSetDictionary[str] = (new string(
                        (from s in str where char.IsLetterOrDigit(s) select s).ToArray()), false);
                }
                this.checkedList = checkedList;
                foreach (var str in checkedList)
                {
                    this.dataSetDictionary[str] = (this.dataSetDictionary[str].Normalized, true);
                }
            }

            public bool GetChecked(string str)
            {
                return this.dataSetDictionary[str].IsChecked;
            }

            public void SetChecked(string str, bool isChecked)
            {
                this.dataSetDictionary[str] = (this.dataSetDictionary[str].Normalized, isChecked);
                if (isChecked)
                {
                    checkedList.Add(str);
                }
                else
                {
                    checkedList.Remove(str);
                }
            }

            public IList<string> GetFiltered(string template)
            {
                if (string.IsNullOrEmpty(template))
                {
                    return this.originDataSet;
                }
                var templateRegex = BuildRegex(template);

                int capacity = this.originDataSet.Count / 4 / template.Length;
                if (capacity < 4)
                {
                    capacity = 4;
                }
                var newList = new List<string>(capacity);
                var query = from str in this.originDataSet
                            where templateRegex.IsMatch(this.dataSetDictionary[str].Normalized)
                            select str;
                newList.AddRange(query);
                return newList;
            }

            Regex BuildRegex(string str)
            {
                str = Regex.Escape(str);
                var res = new List<char>(str.Length);
                int i = 0;
                while (i < str.Length && !char.IsLetterOrDigit(str[i]))
                {
                    i++;
                }
                for (; i < str.Length; i++)
                {
                    if (char.IsLetterOrDigit(str[i]))
                    {
                        res.Add(str[i]);
                        if (i + 1 < str.Length && char.IsUpper(str[i]) && char.IsUpper(str[i + 1]))
                        {
                            res.Add('.');
                            res.Add('*');
                            res.Add('?');
                        }
                    }
                    else
                    {
                        while (++i < str.Length && !char.IsLetterOrDigit(str[i]))
                        { }
                        i--;
                        res.Add('.');
                        res.Add('*');
                        res.Add('?');
                    }
                }
                if (res[str.Length - 1] != '?')
                {
                    res.Add('.');
                    res.Add('*');
                    res.Add('?');
                }
                return new Regex(new string(res.ToArray()), RegexOptions.IgnoreCase);
            }
        }

        public class SimpleFilter : IAdvancedSearchFilter
        {
            readonly IList<string> originDataSet;
            readonly Dictionary<string, bool> dataSetDictionary;
            readonly List<string> checkedList;

            public SimpleFilter(IList<string> originDataSet, List<string> checkedList)
            {
                this.originDataSet = originDataSet;
                this.dataSetDictionary = new Dictionary<string, bool>(originDataSet.Count);
                foreach (string str in originDataSet)
                {
                    this.dataSetDictionary[str] =  false;
                }
                this.checkedList = checkedList;
                foreach (var str in checkedList)
                {
                    this.dataSetDictionary[str] = true;
                }
            }

            public bool GetChecked(string str)
            {
                return this.dataSetDictionary[str];
            }

            public void SetChecked(string str, bool isChecked)
            {
                this.dataSetDictionary[str] = isChecked;
                if (isChecked)
                {
                    checkedList.Add(str);
                }
                else
                {
                    checkedList.Remove(str);
                }
            }

            public IList<string> GetFiltered(string template)
            {
                if (string.IsNullOrEmpty(template))
                {
                    return this.originDataSet;
                }


                int capacity = this.originDataSet.Count / 4 / template.Length;
                if (capacity < 4)
                {
                    capacity = 4;
                }
                var newList = new List<string>(capacity);
                var query = from str in this.originDataSet
                            where str.Contains(template, StringComparison.OrdinalIgnoreCase)
                            select str;
                newList.AddRange(query);
                return newList;
            }
        }
    }

    interface IAdvancedSearchFilter
    {
        public bool GetChecked(string str);
        public void SetChecked(string str, bool isChecked);
        public IList<string> GetFiltered(string template);
    }
}