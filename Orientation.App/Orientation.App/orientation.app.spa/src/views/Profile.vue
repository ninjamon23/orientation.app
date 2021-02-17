<template>
  <div class="container">
    <h1>This is an about page</h1>
    <div>
      <img :src="$auth.user.picture" />
      <h2>{{ $auth.user.name }}</h2>
      <p>{{ $auth.user.email }}</p>
    </div>

    <div>
      <pre>{{ JSON.stringify($auth.user, null, 2) }}</pre>
    </div>
  </div>
</template>

<script>
export default {
  async mounted () {
    // const token = await this.$auth.getIdTokenClaims()
    // console.log('token', token)
    // console.log('auth', this.$auth)
  }
}
</script>
