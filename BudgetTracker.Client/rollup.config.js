import svelte from 'rollup-plugin-svelte';
import resolve from 'rollup-plugin-node-resolve';
import commonjs from 'rollup-plugin-commonjs';
import livereload from 'rollup-plugin-livereload';
import { terser } from 'rollup-plugin-terser';
import progress from 'rollup-plugin-progress';
import babel from 'rollup-plugin-babel';
import sizes from "rollup-plugin-sizes";
import autoPreprocess from 'svelte-preprocess';
import typescript from '@rollup/plugin-typescript';
import css from "rollup-plugin-css-only";
import generate from './generate';

const production = !process.env.ROLLUP_WATCH;

const onwarn = warning => {
	if (warning.code === 'CIRCULAR_DEPENDENCY') {
	  return
	}

	if (warning.message.indexOf('A11y') != -1) {
		return
	}

	if (warning.message.indexOf('Non-existent export') == 0) {
		return
	}

	if (warning.message.indexOf('Use of eval') == 0) {
		return
	}
	
	var cwd = process.cwd() + "/";

	if (warning.loc) {
		var fileName = warning.loc.file.replace(cwd, "");

		console.warn(`(!) ${warning.message} (${fileName}:${warning.loc.line}:${warning.loc.column}):\n${warning.frame}\n\n`)
	}
	else {
		console.warn(`(!) ${warning.message}`)
	}

}	

export default {
	input: 'src/main.js',
	output: {
		sourcemap: true,
		format: 'iife',
		name: 'app',
		file: '../BudgetTracker/wwwroot/js/bundle.js'
	},
	onwarn,
	plugins: [
		css({ output: "../BudgetTracker/wwwroot/css/dashboard.css" }),
		generate(),
		progress(),
		svelte({
			// enable run-time checks when not in production
			dev: !production,
			// we'll extract any component CSS out into
			// a separate file — better for performance
			css: css => {
				css.write('../BudgetTracker/wwwroot/css/bundle.css');
			},
			preprocess: autoPreprocess()
		}),

		// If you have external dependencies installed from
		// npm, you'll most likely need these plugins. In
		// some cases you'll need additional configuration —
		// consult the documentation for details:
		// https://github.com/rollup/rollup-plugin-commonjs
		resolve(),
		commonjs(),
		typescript({ sourceMap: !production }),

		// Watch the `public` directory and refresh the
		// browser on changes when not in production
		!production && livereload('../BudgetTracker/wwwroot/'),
		babel({
			exclude: 'node_modules/**' // only transpile our source code
		  }),
        sizes(),

		// If we're building for production (npm run build
		// instead of npm run dev), minify
		production && terser()
	],
	watch: {
		clearScreen: true
	}
};
