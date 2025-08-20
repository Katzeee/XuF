using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xuf.UI
{
    /// <summary>
    /// Abstract base class for all models that can notify when data changes.
    /// Follows unidirectional data flow pattern where Model changes trigger View updates.
    /// Uses CRTP so concrete models declare their own type for stronger typing.
    /// </summary>
    public abstract class ModelBase<TModel, TData>
        where TModel : ModelBase<TModel, TData>
    {
        private TData _data;

        /// <summary>
        /// Event triggered when data changes
        /// </summary>
        public event Action<TData> OnDataChanged;

        /// <summary>
        /// The data stored in this model
        /// </summary>
        public TData Data { get => _data; }

        /// <summary>
        /// Default constructor for use with FormBase<TData, TModel> pattern
        /// </summary>
        public ModelBase()
        {
            _data = default;
        }

        /// <summary>
        /// Initialize the model with data
        /// Called by FormBase when creating a model instance
        /// </summary>
        public void InitializeData(TData initialData)
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
        public void UpdateData(Action<TData> updateAction)
        {
            if (updateAction == null)
                return;

            var oldData = _data;
            updateAction(_data);
            NotifyDataChanged();
            OnDataUpdated(oldData, _data);
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
        public void UpdateData(TData newData)
        {
            if (EqualityComparer<TData>.Default.Equals(_data, newData))
                return;

            var oldData = _data;
            _data = newData;
            NotifyDataChanged();
            OnDataUpdated(oldData, _data);
        }

        /// <summary>
        /// Manually trigger notification of data change
        /// </summary>
        protected void NotifyDataChanged()
        {
            OnDataChanged?.Invoke(_data);
        }

        /// <summary>
        /// Called when the model is first created and bound to a form
        /// Override this method to perform initialization logic
        /// </summary>
        public abstract void OnModelCreated();

        /// <summary>
        /// Called when the model is about to be destroyed
        /// Override this method to perform cleanup logic
        /// </summary>
        public abstract void OnModelDestroyed();

        /// <summary>
        /// Called when data is updated
        /// Override this method to add custom logic after data changes
        /// </summary>
        /// <param name="oldData">Previous data value</param>
        /// <param name="newData">New data value</param>
        public abstract void OnDataUpdated(TData oldData, TData newData);
    }
}
