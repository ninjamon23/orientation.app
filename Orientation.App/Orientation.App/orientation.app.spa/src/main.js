import Vue from 'vue'
import App from './App.vue'
import './registerServiceWorker'
import router from './router'
import store from './store'

// Other dependenceies
import '../node_modules/bulma/css/bulma.css'
import '../node_modules/bulma-o-steps/bulma-steps.css'
import '../node_modules/bulma-floating-button/dist/css/bulma-floating-button.min.css'
// Import the Auth0 configuration
import { domain, clientId } from './auth_config.json'
// Import the plugin here
import { Auth0Plugin } from './auth/index.js'
// Install the authentication plugin here

import loading from 'vuejs-loading-screen'
Vue.use(loading)

Vue.use(Auth0Plugin, {
  domain,
  clientId,
  onRedirectCallback: appState => {
    router.push(
      appState && appState.targetUrl
        ? appState.targetUrl
        : window.location.pathname
    )
  }
})

Vue.config.productionTip = false

new Vue({
  router,
  store,
  render: h => h(App)
}).$mount('#app')
