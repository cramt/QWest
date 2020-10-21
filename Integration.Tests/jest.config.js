module.exports = {
    coverageProvider: "v8",
    testEnvironment: "node",
    preset: "jest-puppeteer",
    globalSetup: './setup.js',
    globalTeardown: './teardown.js',
    testEnvironment: './puppeteer_environment.js',
};
