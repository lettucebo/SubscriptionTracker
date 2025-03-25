import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/Home.vue'
import Subscriptions from '../views/Subscriptions.vue'
import SubscriptionForm from '../views/SubscriptionForm.vue'
import Calendar from '../views/Calendar.vue'
import Categories from '../views/Categories.vue'

/**
 * Defines all application routes and their corresponding components
 * @type {Array<Object>}
 * @namespace routes
 * @property {Object} Home - Dashboard overview route
 * @property {Object} Calendar - Subscription timeline visualization
 * @property {Object} Subscriptions - Subscription list management
 * @property {Object} Categories - Category configuration
 * @property {Object} CreateSubscription - New subscription form
 * @property {Object} EditSubscription - Existing subscription editor
 */
const routes = [
  /**
   * Application dashboard with key metrics and quick actions
   * @memberof routes
   * @name Home
   */
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  /**
   * Calendar visualization of subscription periods and renewals
   * @memberof routes
   * @name Calendar
   */
  {
    path: '/calendar',
    name: 'Calendar',
    component: Calendar
  },
  /**
   * Primary interface for managing subscription entries
   * @memberof routes
   * @name Subscriptions
   * @property {Function} beforeEnter - Route guard for data loading
   */
  {
    path: '/subscriptions',
    name: 'Subscriptions',
    component: Subscriptions
  },
  /**
   * Category configuration and organization interface
   * @memberof routes
   * @name Categories
   * @property {Function} beforeEnter - Route guard for category validation
   */
  {
    path: '/categories',
    name: 'Categories',
    component: Categories
  },
  /**
   * Subscription creation form with validation
   * @memberof routes
   * @name CreateSubscription
   * @property {Function} beforeEnter - Route guard for category selection
   */
  {
    path: '/subscription-form',
    name: 'CreateSubscription',
    component: SubscriptionForm
  },
  /**
   * Subscription editing form with existing data loading
   * @memberof routes
   * @name EditSubscription
   * @param {string} id - Subscription database identifier
   * @property {Function} beforeEnter - Route guard for subscription loading
   */
  {
    path: '/subscription-form/:id',
    name: 'EditSubscription',
    component: SubscriptionForm
  }
]

/**
 * Central router configuration using HTML5 history mode
 * @type {import('vue-router').Router}
 * @property {string} base - Base application path from environment
 * @property {Array} routes - Collection of route definitions
 * @property {Function} scrollBehavior - Default scroll position management
 */
const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
