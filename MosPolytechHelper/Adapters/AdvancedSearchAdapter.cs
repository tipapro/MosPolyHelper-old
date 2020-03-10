namespace MosPolyHelper.Adapters
{
    using Android.Views;
    using Android.Widget;
    using AndroidX.RecyclerView.Widget;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.RegularExpressions;


    // TODO: add id to viewHolder
    class AdvancedSearchAdapter : RecyclerView.Adapter
    {
        IList<int> dataSet;
        readonly IAdvancedSearchFilter filter;

        public event Action<bool> AllCheckedChanged;

        public AdvancedSearchAdapter(IAdvancedSearchFilter filter)
        {
            this.filter = filter;
            this.dataSet = filter.GetFiltered(null);
        }

        public override int ItemCount => this.dataSet?.Count ?? 0;

        public void UpdateTemplate(string template)
        {
            this.dataSet = this.filter.GetFiltered(template);
            NotifyDataSetChanged();
            AllCheckedChanged?.Invoke(IsAllChecked());
        }

        public bool IsAllChecked()
        {
            return this.filter.IsAllChecked(this.dataSet);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_advanced_search, parent, false);
            var vh = new ViewHolder(view, CheckBoxChanged);
            return vh;
        }

        void CheckBoxChanged(int position, bool isChecked)
        {
            this.filter.SetChecked(this.dataSet[position], isChecked);
            AllCheckedChanged?.Invoke(IsAllChecked());
        }

        public void SetCheckAll(bool flag)
        {
            for (int i = 0; i < this.dataSet.Count; i++)
            {
                this.filter.SetChecked(this.dataSet[i], flag);
            }
            NotifyDataSetChanged();
        }


        public override void OnBindViewHolder(RecyclerView.ViewHolder vh, int position)
        {
            if (!(vh is ViewHolder viewHolder))
            {
                return;
            }
            viewHolder.CheckBox.Text = this.filter.GetValue(this.dataSet[position]);
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
            readonly bool[] checkedArray;
            readonly string[] normalized;
            readonly ObservableCollection<int> checkedIndexes;

            public bool IsAllChecked(IList<int> localDataSet)
            {
                if (localDataSet.Count > checkedIndexes.Count)
                {
                    return false;
                }
                foreach (var index in localDataSet)
                {
                    if (!GetChecked(index))
                    {
                        return false;
                    }
                }
                return true;
            }

            public AdvancedFilter(IList<string> originDataSet, ObservableCollection<int> checkedList)
            {
                this.originDataSet = originDataSet;
                if (this.originDataSet != null)
                {
                    this.normalized = new string[originDataSet.Count];
                    for (int i = 0; i < this.normalized.Length; i++)
                    {
                        this.normalized[i] = new string(
                            (from s in originDataSet[i] where char.IsLetterOrDigit(s) select s).ToArray());
                    }

                    this.checkedArray = new bool[originDataSet.Count];
                    this.checkedIndexes = checkedList;
                    foreach (int value in checkedList)
                    {
                        this.checkedArray[value] = true;
                    }
                }
            }

            public string GetValue(int index)
            {
                return this.originDataSet[index];
            }

            public bool GetChecked(int index)
            {
                return this.checkedArray[index];
            }

            public void SetChecked(int index, bool isChecked)
            {
                if (this.checkedArray[index] == isChecked)
                {
                    return;
                }
                this.checkedArray[index] = isChecked;
                if (isChecked)
                {
                    this.checkedIndexes.Add(index);
                }
                else
                {
                    this.checkedIndexes.Remove(index);
                }
            }

            public IList<int> GetFiltered(string template)
            {
                if (string.IsNullOrEmpty(template) || this.originDataSet == null)
                {
                    int[] array = new int[this.originDataSet.Count];
                    for (int i = 0, j = 0; i < array.Length; i++)
                    {
                        if (GetChecked(i))
                        {
                            array[i] = array[j];
                            array[j] = i;
                            j++;
                        }
                        else
                        {
                            array[i] = i;
                        }
                    }
                    return array;
                }
                var templateRegex = BuildRegex(template);

                int capacity = this.originDataSet.Count / 4 / template.Length;
                if (capacity < 4)
                {
                    capacity = 4;
                }
                var newList = new List<int>(capacity);
                for (int i = 0, j = 0; i < this.originDataSet.Count; i++)
                {
                    if (templateRegex.IsMatch(this.normalized[i]))
                    {
                        newList.Add(i);
                        if (GetChecked(newList[^1]))
                        {
                            int buf = newList[j];
                            newList[j] = newList[^1];
                            newList[^1] = buf;
                            j++;
                        }
                    }
                }
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
            readonly bool[] checkedArray;
            readonly ObservableCollection<int> checkedIndexes;

            public bool IsAllChecked(IList<int> localDataSet)
            {
                if (localDataSet.Count > checkedIndexes.Count)
                {
                    return false;
                }
                foreach (var index in localDataSet)
                {
                    if (!GetChecked(index))
                    {
                        return false;
                    }
                }
                return true;
            }

            public SimpleFilter(IList<string> originDataSet, ObservableCollection<int> checkedList)
            {
                this.originDataSet = originDataSet;
                if (this.originDataSet != null)
                {
                    this.checkedArray = new bool[originDataSet.Count];
                    this.checkedIndexes = checkedList;
                    foreach (int value in checkedList)
                    {
                        this.checkedArray[value] = true;
                    }
                }
            }

            public string GetValue(int index)
            {
                return this.originDataSet[index];
            }

            public bool GetChecked(int index)
            {
                return this.checkedArray[index];
            }

            public void SetChecked(int index, bool isChecked)
            {
                if (this.checkedArray[index] == isChecked)
                {
                    return;
                }
                this.checkedArray[index] = isChecked;
                if (isChecked)
                {
                    this.checkedIndexes.Add(index);
                }
                else
                {
                    this.checkedIndexes.Remove(index);
                }
            }

            public IList<int> GetFiltered(string template)
            {
                if (string.IsNullOrEmpty(template) || this.originDataSet == null)
                {
                    int[] array = new int[this.originDataSet.Count];
                    for (int i = 0, j = 0; i < array.Length; i++)
                    {
                        if (GetChecked(i))
                        {
                            array[i] = array[j];
                            array[j] = i;
                            j++;
                        }
                        else
                        {
                            array[i] = i;
                        }
                    }
                    return array;
                }


                int capacity = this.originDataSet.Count / 4 / template.Length;
                if (capacity < 4)
                {
                    capacity = 4;
                }
                var newList = new List<int>(capacity);
                for (int i = 0, j = 0; i < this.originDataSet.Count; i++)
                {
                    if (this.originDataSet[i].Contains(template, StringComparison.OrdinalIgnoreCase))
                    {
                        newList.Add(i);
                        if (GetChecked(newList[^1]))
                        {
                            int buf = newList[j];
                            newList[j] = newList[^1];
                            newList[^1] = buf;
                            j++;
                        }
                    }
                }
                return newList;
            }
        }
    }

    interface IAdvancedSearchFilter
    {
        public bool IsAllChecked(IList<int> localDataSet);
        public string GetValue(int index);
        public bool GetChecked(int index);
        public void SetChecked(int index, bool isChecked);
        public IList<int> GetFiltered(string template);
    }
}