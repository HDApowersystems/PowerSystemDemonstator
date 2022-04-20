from setuptools import setup, find_packages

setup(name='power_network_lib',
      version='0.1',
      description='power_network_simulator',
      url='http://example.com',
      author='Sharareh',
      author_email='sh@example.com',
      license='MIT',
      packages=find_packages(where='power_network_lib'),
      install_requires=[
          'pandapower',
      ],
      # entry_points={
      #     'console_scripts': ['power-network-lib=power_network_lib.power_network:get_instance'],
      # },
      zip_safe=False)
