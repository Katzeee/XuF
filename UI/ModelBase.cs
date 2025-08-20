using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xuf.UI
{
    /// <summary>
    /// Generic model class that can notify when data changes.
    /// Follows unidirectional data flow pattern where Model changes trigger View updates.
    /// This is a pure Model that only manages data state and notifications.
    /// Can be used directly without subclassing for simple data models.
    /// </summary>
    public class ModelBase<T>
    {
        private T _data;

        /// <summary>
        /// Event triggered when data changes
        /// </summary>
        public event Action<T> OnDataChanged;

        /// <summary>
        /// The data stored in this model
        /// </summary>
        public T Data { get => _data; }

        /// <summary>
        /// Constructor with initial data
        /// </summary>
        public ModelBase(T initialData = default)
        {
            _data = initialData;
        }

        /// <summary>
        /// Updates specific properties of the data object without replacing the entire object.
        /// This allows for partial updates while still notifying all observers of the change.
        /// Use this method when you want to modify part of your data and have the view automatically update.
        /// </summary>
        /// <param name="updateAction">Action that modifies the data object</param>
        /// <example>
        /// UpdateData(data => {
        ///     data.health = newHealth;
        ///     data.score += points;
        /// });
        /// </example>
        public void UpdateData(Action<T> updateAction)
        {
            if (updateAction == null)
                return;
            updateAction(_data);
            NotifyDataChanged();
        }


        /// <summary>
        /// Updates the data by directly replacing it with a new instance.
        /// This method will notify all observers of the change if the new data is different.
        /// </summary>
        /// <param name="newData">The new data instance to use</param>
        /// <example>
        /// var newPlayerData = new PlayerData { health = 100, name = "Player1" };
        /// UpdateData(newPlayerData);
        /// </example>
        public void UpdateData(T newData)
        {
            if (EqualityComparer<T>.Default.Equals(_data, newData))
                return;


            _data = newData;
            NotifyDataChanged();
        }

        /// <summary>
        /// Manually trigger notification of data change
        /// </summary>
        protected void NotifyDataChanged()
        {
            OnDataChanged?.Invoke(_data);
        }
    }
}
