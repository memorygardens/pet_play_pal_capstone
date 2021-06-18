<template>
	<div id="app" class="darkmode">
		<header class="bubble">
			<router-link
				id="home-nav"
				v-bind:to="{ name: 'home' }"
				class="green-button"
				>Pet Play Pals</router-link
			>
			<div id="right-header">
				<router-link
					class="green-button"
					v-bind:to="{ name: 'login' }"
					v-if="$store.state.token === ''"
					>Login</router-link
				>
				<div v-if="$store.state.token != ''">
					<router-link
						class="green-button"
						v-bind:to="{ name: 'profile' }"
						>Profile</router-link
					>
					<router-link
						class="green-button"
						v-bind:to="{ name: 'logout' }"
						>Logout</router-link
					>
				</div>
			</div>
		</header>
		<aside class="bubble foggy-gray-bg">
			<img id="logo" src="./../img/newLogo.png" alt="pet play pals logo" class="bubble">
			<router-link v-bind:to="{ name: 'playdates' }" id="search" class="green-button">Search Playdates</router-link>
			<!-- <router-link v-bind:to="{ name: '???' }" id="forum" class="green-button" v-if="$store.state.token != ''">Forum</router-link> -->
			<router-link v-bind:to="{ name: 'createPlaydate' }" id="schedule" class="green-button" v-if="$store.state.token != ''">Schedule Playdate</router-link>
		</aside>
		<main class="bubble foggy-gray-bg">
			<router-view class="main" />
		</main>
		<footer class="bubble foggy-gray-bg">&copy; Pet Play Pals</footer>
	</div>
</template>
<script>
import "./cssStyles/style.css";
import "./cssStyles/header.css";
import "./cssStyles/leftBar.css";
export default {
	created() {
		// INITIALIZE_LOCAL_STATIC_DB_CACHE(state) { 
		// },


		this.$store.commit("INITIALIZE_LOCAL_STATIC_DB_CACHE");
	},
};
</script>




<style scoped>
#logo{
	height: 250px;
	width: 250px;
	margin: 6px;
}
header {
	grid-area: header;
}
aside {
	grid-area: aside;
}
main {
	grid-area: main;
}
footer {
	grid-area: footer;
}

#app {
	display: grid;
	grid-template-columns: auto 1fr 1fr;
	grid-template-areas:
		"header header header"
		"aside main main"
		"footer footer footer";
	grid-gap: 10px;
	background-color: 0d7685;
}

header {
	font-family: "Avenir", Helvetica, Arial, sans-serif;
	-webkit-font-smoothing: antialiased;
	-moz-osx-font-smoothing: grayscale;
	text-align: center;
	display: flex;
	justify-content: space-between;
}
</style>